
# Help us with Localization!

Localization of the New Backoffice is in full swing!
This is a work in process and here you can find the overview of all the sections that needs to be localized. We are also looking forward to see any contributions towards localization of the new Backoffice.


You may tick a section/subsection in the same PR as your changes, if it completes said section.


Before you start:
- Make sure you have read the [README](https://github.com/umbraco/Umbraco.CMS.Backoffice/blob/main/.github/README.md) and [Contributing Guidelines](https://github.com/umbraco/Umbraco.CMS.Backoffice/blob/main/.github/CONTRIBUTING.md).
- Please note some sections may already be partly or fully localized without it being reflected in the overview just yet.
- Get an understanding of how we do localization in the new Backoffice. The explanations can be found in the stories under **Localization** by running `npm run storybook`. Alternatively see the raw story file [localization.mdx](https://github.com/umbraco/Umbraco.CMS.Backoffice/blob/main/src/packages/core/localization/stories/localization.mdx) 



# Overview
 - [Sections that needs to be localized](#sections)
 - [Keys that needs to be localized](#keys)

## Sections

- [ ] [Header App](#header-app)
- [ ] [Content](#content)
- [ ] [Media](#media)
- [ ] [Settings](#settings)
- [ ] [Members](#members)
- [ ] [Packages](#packages)
- [ ] [Dictionary](#dictionary)
- [ ] [Users](#users)
- [ ] [Property Editors](#property-editor-ui-and-their-input)
- [ ] [Modals](#modals)
- [ ] [Misc](#misc)


### Subsections

#### Header App
- [ ] Ensure all sections are localized
- [ ] Search
- [ ] Current user (Modal)
  - [ ] Change password

#### Content
- [ ] Dashboards
	- [ ] Welcome
	- [ ] Redirect Management
- [ ] Content / Document
	- [ ] Section: Content
	- [ ] Section: Info
	- [ ] Section: Actions

#### Media
- [ ] (To be continued)

#### Settings
- [ ] Dashboards
	- [x] Welcome / Settings
	- [ ] Examine Management
	- [ ] Models Builder
	- [ ] Published Status
	- [ ] Health Check
	- [x] Profiling
	- [x] Telemetry Data
- [ ] Document Type
	- [ ] Section: Design
	- [ ] Section: Structure
	- [ ] Section: Settings
	- [ ] Section: Templates
- [ ] Media Type
- [ ] Member Type
- [ ] Data Type
	- [ ] Section: Details
	- [ ] Section: Info
- [ ] Relation Types
- [ ] Log Viewer
- [ ] Document Blueprints
- [ ] Languages
- [ ] Extensions
- [ ] Templates
- [ ] Partial Views
- [ ] Stylesheets
	- [ ] Section: Rich Text Editor
	- [ ] Section: Code
- [ ] Scripts

#### Members
- [ ] Member Groups
- [ ] Members

#### Packages
- [ ] Section: Installed
- [ ] Section: Created
	- [ ] Package builder: "Create Package" 

#### Dictionary
- [ ] Everything within Dictionary

#### Users
- [ ] Users
- [ ] User Groups
- [ ] Create user
- [ ] User Profiles

#### Property Editor UI (and their inputs)
Ensure all property editors are properly localized.
(Some may be missing in this list / more to be added)
- [ ] Block Grid
- [ ] Block List
- [x] Checkbox List
- [ ] Collection View
- [ ] Color Picker
- [ ] Date Picker
- [x] Dropdown
- [ ] Eye Dropper
- [x] Icon Picker
- [ ] Image Cropper
- [ ] Image Crops Configuration
- [x] Label
- [ ] Markdown Editor
- [ ] Media Picker
- [ ] Member Group Picker
- [ ] Member Picker
- [ ] Multi URL Picker
- [ ] Multiple Text String
- [ ] Number (missing label)
- [ ] Number Range
- [ ] Order Direction
- [x] Radio Button List
- [ ] Slider (label)
- [ ] TextBox (label)
- [ ] TextArea
- [ ] TinyMCE
- [ ] Toggle
- [ ] Tree Picker
	- [ ] StartNode
	- [x] DynamicRoot
- [ ] Upload Field
- [ ] User Picker
- [ ] Value Type

#### Modals
Ensure all modals are properly localized.
(Some may be missing in this list / more to be added) 
- [ ] Code Editor
- [ ] Confirm
- [ ] Embedded Media
- [ ] Folder
- [ ] Icon Picker
- [ ] Link Picker
- [ ] Property Settings
- [ ] Section Picker
- [ ] Template
- [ ] Tree Picker
- [ ] Debug

Rest of modals can be found:
- [ ] Umb***ModalName***ModalElement


#### Misc

- [ ] Tree
	- [ ] Tree Actions
	- [ ] Recycle Bin
- [ ] Validator messages


## Keys

Do you speak any of the following languages?
Then we need your help! With Bellissima we added new localization keys, and we still need them available in all our supported languages.

- `bs-BS` - Bosnian (Bosnia and Herzegovina)
- `cs-CZ` - Czech (Czech Republic)
- `cy-GB` - Welsh (United Kingdom)
- `da-DK` - Danish (Denmark)
- `de-DE` - German (Germany)
- `en-GB` - English (United Kingdom)
- `es-ES` - Spanish (Spain)
- `fr-FR` - French (France)
- `he-IL` - Hebrew (Israel)
- `hr-HR` - Croatian (Croatia)
- `it-IT` - Italian (Italy)
- `ja-JP` - Japanese (Japan)
- `ko-KR` - Korean (Korea)
- `nb-NO` - Norwegian Bokmål (Norway)
- `nl-NL` - Dutch (Netherlands)
- `pl-PL` - Polish (Poland)
- `pt-BR` - Portuguese (Brazil)
- `ro-RO` - Romanian (Romania)
- `ru-RU` - Russian (Russia)
- `sv-SE` - Swedish (Sweden)
- `tr-TR` - Turkish (Turkey)
- `ua-UA` - Ukrainian (Ukraine)
- `zh-CN` - Chinese (China)
- `zh-TW` - Chinese (Taiwan)

#### settingsDashboard
- documentationHeader
- documentationDescription
- communityHeader
- trainingHeader
- trainingDescription
- supportHeader
- supportDescription
- videosHeader
- videosDescription
- getHelp
- getCertified
- goForum
- chatWithCommunity
- watchVideos

- [ ] `bs-BS` - Bosnian (Bosnia and Herzegovina)
- [ ] `cs-CZ` - Czech (Czech Republic)
- [ ] `cy-GB` - Welsh (United Kingdom)
- [x] `da-DK` - Danish (Denmark)
- [ ] `de-DE` - German (Germany)
- [ ] `en-GB` - English (United Kingdom)
- [ ] `es-ES` - Spanish (Spain)
- [ ] `fr-FR` - French (France)
- [ ] `he-IL` - Hebrew (Israel)
- [ ] `hr-HR` - Croatian (Croatia)
- [ ] `it-IT` - Italian (Italy)
- [ ] `ja-JP` - Japanese (Japan)
- [ ] `ko-KR` - Korean (Korea)
- [ ] `nb-NO` - Norwegian Bokmål (Norway)
- [ ] `nl-NL` - Dutch (Netherlands)
- [ ] `pl-PL` - Polish (Poland)
- [ ] `pt-BR` - Portuguese (Brazil)
- [ ] `ro-RO` - Romanian (Romania)
- [ ] `ru-RU` - Russian (Russia)
- [ ] `sv-SE` - Swedish (Sweden)
- [ ] `tr-TR` - Turkish (Turkey)
- [ ] `ua-UA` - Ukrainian (Ukraine)
- [ ] `zh-CN` - Chinese (China)
- [ ] `zh-TW` - Chinese (Taiwan)
