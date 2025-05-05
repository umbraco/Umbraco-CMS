# **Contributing to Localization in the Backoffice**

Do you want to help keep our translations accurate and up to standard? 🌍✨

Your input makes a real difference! By reviewing, refining, or suggesting improvements, you ensure that our translations remain clear, consistent, and user-friendly for everyone.


## **How Can I Contribute?**

To contribute to localization in the Backoffice, follow this step-by-step guide:


### **1. Change the Language in Backoffice**



1. Open the Backoffice, click on your profile icon in the top-right corner, and select "Edit." 



![Menubar profile image](/.github/img/contributing/ProfileImage.png "Menubar profile image")



![Edit button inside profile](/.github/img/contributing/editBtn.png "Edit button inside profile")


2. Under "UI Culture," select the language you want to review from the dropdown menu. 



![Dropdown of languages in Umbraco](/.github/img/contributing/uiCulture.png "Dropdown of languages in Umbraco")



### **2. Find a Translation Error**



1. Navigate through the Backoffice and check if everything is translated correctly. 

2. When you find a translation error, right-click on it and select "Inspect." 

3. Look for the nearest element that starts with `umb-` and has a name indicating something specific to the given location. 
 
    **Example:**  

    * The closest parent element should be specific, such as `umb-document-type-workspace-view-settings` instead of a generic element like `umb-property-layout.`


### **3. Find the Code in VS Code**



1. Open VS Code and search for the nearest `umb-` element you identified. 



![Search for nearest umb element in code](/.github/img/contributing/searchInVsCode.png "Search for nearest umb element in code")

2. Scroll down to find `render() {` and look for the element label that needs updating. 


![Find render and label in code](/.github/img/contributing/renderCode.png "Find render and label in code")

3. If the label is hardcoded, it must be updated. 
 
    **Example:**  
    `label="Vary by culture"`


### **4. Find the Correct Translation**



1. Open the `en.ts` or `en-us.ts` file and search for relevant keywords. \
 \
    **Example:** 

    * If the text is "Vary by culture," search for `vary`, `culture`, or `Vary by culture`. 


2. Once you find the translation, take the element name and search for it in the target language file (e.g., `da-dk.ts` for Danish). 


![Search for translation in language files](/.github/img/contributing/searchingThroughLanguageFiles.png "Search for translation in language files")

3. If a translation exists, insert it into the label element found earlier. 



### **5. Insert the Translation**

To display the new translation correctly, insert the following code inside the label element:

`${this.localize.term('action_key')}`

Replace `action_key` with the correct translation key.

**Example:**

`${this.localize.term('contentTypeEditor_allowVaryByCulture')}`

![Localization code snippet](/.github/img/contributing/localizationCodeSnippetInCode.png "Localization code snippet")


Save the changes and return to the Backoffice to see the update.


![Changes in backoffice after changes in code](/.github/img/contributing/changedBackofficeAfterLocalization.png "Changes in backoffice after changes in code")



### **6. Commit and Push**



1. Commit your changes to a new temporary branch (avoid committing directly to `contrib`). 

2. Push the changes to your fork on GitHub.


### **7. Create a Pull Request**



1. In your forked repository on GitHub (`https://github.com/[YourUsername]/Umbraco-CMS`), a banner will appear stating that you pushed a new branch. 

2. Click the button to create a pull request and follow the instructions.


## **I Can’t Find the Correct Translation**

If you can’t find the translation you need, it may not exist yet. In this case, you can create a new action with related keys.


### **1. Ensure It Doesn’t Already Exist**

Search thoroughly in `en.ts` or `en-us.ts` for all relevant keywords.


### **2. Create an Action**



1. Choose a meaningful name for the action to avoid confusion. \
 \
 **Example:** Translation for the Data Type "Color Picker." 

    * **Good name:** `colorPickerConfigurations`
    * **Bad name:** `colorpicker`
2. A specific action name prevents unnecessarily long key names. 

3. Define the action: 



### **3. Create Keys**



1. Use clear and descriptive key names. \
 \
 **Example:**
    * **Good name:** `colorsTitle`
    * **Bad name:** `colors`
2. Add the necessary keys inside the action with proper translations. 



![Action and related Keys in language files](/.github/img/contributing/actionAndKeys.png "Action and related Keys in language files")



## **I Can’t Find a <code>render()</code> Code in VS Code**

In some cases, such as Data Types, the label might not be inside `render()`. Instead, it may be in a manifest file.


### 1. Search for the Text

Copy the text from the Backoffice and search for it in the code.


### 2. Open the Manifest File

Once you find the relevant manifest file, open it to confirm you’re in the right place.


### 3. Change the Label

In Markdown files, localization is slightly different. Instead of:
`${this.localize.term('action_key')}`

Use: `#action_key`

**Example:**
`#colorPickerConfigurations_showLabelTitle`

### 4. Change the Description

For descriptions in Markdown files, use:
`{umbLocalize: action_key}`

**Example:**
`{umbLocalize: colorPickerConfigurations_showLabelDescription}`


### 5. Save and Verify

Once all changes are made, your manifest should look something like this:


![Localization changes to manifest files](/.github/img/contributing/finishedManifestAfterLocalizatonChanges.png "Localization changes to manifest files")



---


### Thank you 

Following these steps ensures that the Umbraco Backoffice remains accessible and user-friendly in all supported languages. Thanks for contributing! 🎉
