# Umbraco static assets

The files in this directory are static assets that are used by the Umbraco backoffice, installer, public-facing sites, and login screen. These files are served by the Umbraco backend and are not intended to be used by the front-end of your website.

## Structure

The assets are structured in the following way:

| Name | Description                                                        | Usage                                                                           |
| ---- |--------------------------------------------------------------------|---------------------------------------------------------------------------------|
| `login.jpg` | The background image for the login screen.                         | /umbraco/management/api/v1/security/back-office/graphics/login-background       |
| `logo.svg` | The Umbraco logo for the Backoffice and other public facing sites. | /umbraco/management/api/v1/security/back-office/graphics/logo                   |
| `logo_dark.svg` | The Umbraco logo in dark mode for the login screen.                | /umbraco/management/api/v1/security/back-office/graphics/login-logo-alternative |
| `logo_light.svg` | The Umbraco logo in light mode for the login screen.               | /umbraco/management/api/v1/security/back-office/graphics/login-logo             |

All assets are linked up through the BackOfficeGraphicsController which uses the constants defined in [ContentSettings](../../../../Umbraco.Core/Configuration/Models/ContentSettings.cs).
