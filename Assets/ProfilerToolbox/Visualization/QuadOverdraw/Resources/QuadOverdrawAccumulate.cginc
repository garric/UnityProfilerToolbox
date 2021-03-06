// Because we use the same UAV for Descritors and Complexity, we have the same clear value.
// In this case, the invalid descriptor is 0, so we need to add 1 to the primitive ID to prevent clash.
const static uint QO_DESC_NONE = 0;

// Those are the states for the quad overdraw processing (QOS)
const static int QOS_ABORT = -2;
const static int QOS_DONE = -1;
const static int QOS_PENDING = 0;
const static int QOS_SYNCHRONIZING = 1;
const static int QOS_OWNER = 2;

uint GetPrimitiveID(uint Desc)
{
    return (Desc >> 2) - 1;
}

uint GetQuadPixelCount(uint Desc)
{
    return Desc & 0x3;
}

uint GenerateDesc(uint PID)
{
    return (PID + 1) << 2;
}

// The temporary buffer used to synchronize and exchange data between quad sub-pixels.
// Left half hold QuadDescriptor, right half hold QuadComplexity
// Both are halfres here.
RWTexture2D<uint> RWQuadBuffer : register(u1);

// The QuadComplexity is the same resource as the QuadDescriptor with an offset in X.
// This is only required to prevent binding an additional UAV which would exceed the allowed RT + UAV count.
#define RWQuadDescriptor RWQuadBuffer
#define RWQuadComplexity RWQuadBuffer

uniform float4 _RWQuadBuffer_Dimension;

uint2 QuadComplexityOffset()
{
    //uint QuadBufferWidth, QuadBufferHeight;
    //RWQuadBuffer.GetDimensions(QuadBufferWidth, QuadBufferHeight);
    //return uint2(QuadBufferWidth / 2, 0);
    return uint2(_RWQuadBuffer_Dimension.x / 2, 0);
}

uint ComputeQuadCoverage(uint2 SvPosition, uint PID, uniform int NumIteration, uniform bool bOwnerOnly, uniform bool bOutputToComplexity, uint QuadComplexity/*Not Used Now*/)
{
    uint2 QuadID = SvPosition.xy / 2;
    int State = QOS_PENDING;

    // Excluding the owner. By default we put the maximum value so that if failed to process, the scale will be one.
    // Starting with 3 also enables a quick completion if all pixels are updated. (once at 3, it can not be increased furthermore)
    uint QuadPixelCount = 32;

    // Because several primitives could be accessing the same quad, we need to loop enough iteration for everything to sync.
    [loop]
    for (int i = 0; i < NumIteration; i++)
    {
        // When outputting to complexity, we assume there are no valid rendertarget update, aside depth buffer.
        // Depth to still be updated correctly, the shader calling this needs to activate [earlydepthstencil]
        if (bOutputToComplexity)
        {
            clip(State);
        }

        [branch]
        if (!bOwnerOnly && State == QOS_SYNCHRONIZING) // bOwnerOnly don't use this path.
        {
            uint CurrDesc = RWQuadDescriptor[QuadID];

            // If the primitive ID has changed, then the owner as finished his process.
            [flatten]
            if (GetPrimitiveID(CurrDesc) != PID)
            {
                State = QOS_DONE;
            }
            else
            {
                QuadPixelCount = GetQuadPixelCount(CurrDesc);
            }
        }
    
        [branch]
        if (State == QOS_OWNER)
        {
            uint CurrCount = GetQuadPixelCount(RWQuadDescriptor[QuadID]);

            // If the count is not increasing, stop now.
            [branch]
            if (CurrCount == QuadPixelCount)
            {
                RWQuadDescriptor[QuadID] = QO_DESC_NONE;
                State = QOS_DONE;

                if (bOutputToComplexity)
                {
                    InterlockedAdd(RWQuadComplexity[QuadID + QuadComplexityOffset()], 1/*QuadComplexity*/);
                }
            }
            else
            {
                QuadPixelCount = CurrCount;
            }
        }

        [branch]
        if (State == QOS_PENDING)
        {
            uint PrevDesc = 0;
            InterlockedCompareExchange(RWQuadDescriptor[QuadID], QO_DESC_NONE, GenerateDesc(PID), PrevDesc);

            // If no primitive was processing this quad, then this pixel owns it.
            [flatten]
            if (PrevDesc == QO_DESC_NONE)
            {
                State = QOS_OWNER;
            }

            // If another pixel from the same primitive is the owner, start synchronizing.
            [branch]
            if (GetPrimitiveID(PrevDesc) == PID)
            {
                InterlockedAdd(RWQuadDescriptor[QuadID], 1);

                State = bOwnerOnly ? QOS_ABORT : QOS_SYNCHRONIZING;
            }
        }
    }

    // This is required in case the number of iteration was too small, release the ownership of the quad.
    [branch]
    if (State == QOS_OWNER)
    {
        RWQuadDescriptor[QuadID] = QO_DESC_NONE;
    }

    if (bOutputToComplexity)
        return 0;
    else
        return State != QOS_ABORT ? (1 + QuadPixelCount) : 0;    
}