using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using DocumentFormat.OpenXml.Packaging;

namespace ShapeCrawler.Presentations;

/// <summary>
///     Tracks all media parts to enable de-duplication.
/// </summary>
/// <remarks>
///     Currently only tracks image parts, but can be extended in the future to all media types.
/// </remarks>
internal class MediaCollection
{
    /// <summary>
    ///     Reference to every known image part by the hash of its data stream.
    /// </summary>
    private readonly Dictionary<string, ImagePart> imagePartsByHash = [];

    /// <summary>
    ///     Compute the hash for a given file.
    /// </summary>
    /// <param name="fileStream">Stream of data contents.</param>
    /// <returns>Base64 hash for that file.</returns>
    public static string ComputeFileHash(Stream fileStream)
    {
        using var sha512 = SHA512.Create();
        fileStream.Position = 0;
        var hash = sha512.ComputeHash(fileStream);
        fileStream.Position = 0;

        return System.Convert.ToBase64String(hash);
    }

    /// <summary>
    ///     Gets the image part associated with the specified file contents hash.
    /// </summary>
    /// <param name="hash">Hash generated by `ComputeFileHash()`.</param>
    /// <param name="part">ImagePart coresponding to the given hash, if found.</param>
    /// <returns>True if a part was foumd for the specified hash.</returns>
    public bool TryGetImagePart(string hash, out ImagePart part) => this.imagePartsByHash.TryGetValue(hash, out part!);


    /// <summary>
    ///     Sets the image part for a given file contents hash.
    /// </summary>
    /// <param name="hash">Hash generated by `ComputeFileHash()`.</param>
    /// <param name="part">ImagePart coresponding to the given hash.</param>
    public void SetImagePart(string hash, ImagePart part) => this.imagePartsByHash[hash] = part;
}
