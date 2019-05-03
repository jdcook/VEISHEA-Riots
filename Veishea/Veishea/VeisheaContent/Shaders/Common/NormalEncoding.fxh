#ifndef NORMALENCODING_FXH
#define NORMALENCODING_FXH

// encodeNormals
// Encodes a 3 component normal vector into a 2 component vector
// via Lambert Azimuthal Equal Area Projection.
// Input must be prenormalized.
half2 encodeNormals(half3 input)
{
    half f = sqrt(8*input.z+8);
    return input.xy / f + 0.5;
}

// decodeNormals
// Decodes a 2 component Lambert Azimuthal Equal Area Projection
// encoded vector into the original 3 component normal.
// Resulting vector is normalized.
half3 decodeNormals(half2 enc)
{
    half2 fenc = enc*4-2;
    half f = dot(fenc,fenc);
    half g = sqrt(1-f/4);
    half3 n;
    n.xy = fenc*g;
    n.z = 1-f/2;
    return n;
}

#endif