---
name: 🔐 Security Issue
about: Discovered a Security Issue in Umbraco?
---

⚠️ PLEASE DON'T DISCLOSE SECURITY-RELATED ISSUES PUBLICLY, SEE BELOW.

If you have found a security issue in Umbraco, please send the details to
security@umbraco.com and don't disclose it publicly until we can provide a fix for
it. If you wish, we'll credit you for finding verified issues, when we release
the patched version.

❗ Please read more about how to report security issues on https://umbraco.com/security

A note on "Self XSS"
--------------------

Umbraco is a CMS, that allows users to edit content on a website. As such,
all _authenticated users_ can:

 - Edit content, and (depending on the field types) insert HTML and CSS in that
   content, with a variety of allowed attributes.
 - Depending on the user level: Edit template files, and insert C#, HTML, CSS and
   javascript in so on.
 - Upload files to the site, which will become publicly available.

We see these functionalities as _features_, and not as security issues. Please
report the mentioned items only if they can be performed by non-authorized
users, or other exploitable vulnerabilities.

Thanks!
