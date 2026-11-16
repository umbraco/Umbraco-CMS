# Six Labors licence key

`sixlabors.lic` is the Six Labors licence key that the `SixLabors.ImageSharp` and `SixLabors.ImageSharp.Web` packages validate at build time. Release builds fail without a valid key; Debug builds only warn. The project points at the file through the `SixLaborsLicenseFile` property in `Umbraco.Cms.Imaging.ImageSharp.csproj`.

| | |
|---|---|
| License ID | `ctm_01kwvyngdh9te11b5pqern2w9c` |
| License Type | Community (assembly-scoped) |
| Assembly | `Umbraco.Cms.Imaging.ImageSharp` |
| Valid until | 10-01-2028 |

The key is assembly-scoped, so it is valid only for the assembly named above and may be committed to this public repository. Use the file exactly as issued; do not edit its contents. A new project that references a Six Labors package directly must have its assembly name added to the key when it is renewed.

## Renewal

Renew in the final 90 days before expiry (from 12-10-2027) by resubmitting the form at https://licensing.sixlabors.com/ from the same email address used for the original application, selecting **OpenSource** and listing the exact assembly names. Replace `sixlabors.lic` with the new file and update the details above.

## Background

See the Six Labors announcement, [Security Support and License Enforcement](https://sixlabors.com/posts/security-support-and-license-enforcement/).
