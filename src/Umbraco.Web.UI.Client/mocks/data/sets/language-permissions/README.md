# Mock set: Language Permissions

Click through how the document editor (Content section) and the element editor (Library section) behave for users with different language access and different access to shared data.
The same scenarios are checked automatically by the acceptance tests in `tests/Umbraco.Tests.AcceptanceTest/tests/Shared/LanguageAccess/`.

## How to use it

1. Run `npm run dev:mock`.
2. In the header, open **Mock: ...** and pick **Language Permissions**.
3. In the header, open **User: ...** and pick the user to try. The page reloads as that user.
4. Open a document in the Content section, or an element in the Library section, and switch language (the language picker in the header, or the app language in the sidebar).

## The users

There is one user and one user group for every combination of languages and shared data access. The user and the group have the same name, so you can see which scenario you are in from the name alone.

| Languages the user may edit     | Shared data             |
| ------------------------------- | ----------------------- |
| All languages                   | can edit shared data    |
| All languages                   | cannot edit shared data |
| English only (default language) | can edit shared data    |
| English only (default language) | cannot edit shared data |
| Danish only                     | can edit shared data    |
| Danish only                     | cannot edit shared data |
| English and Danish              | can edit shared data    |
| English and Danish              | cannot edit shared data |
| No languages                    | can edit shared data    |
| No languages                    | cannot edit shared data |

Vietnamese is never granted individually, so it is the language a restricted user has no access to. **Shared data** is data that does not vary by language.

## The documents

| Document                                                                    | What it is for                                                                                                                                                                                                                      |
| --------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Page with text per language and shared text (English / Danish / Vietnamese) | One page with three languages. It has a **Text per language** (one value per language) and a **Shared text** (one value for all languages).                                                                                         |
| Page that does not vary by language                                         | A page with one version and one **Text**. It is never restricted by language.                                                                                                                                                       |
| Page with a shared block list (English / Danish / Vietnamese)               | One page with three languages and a **Shared block list** holding one block that varies by language, with a **Text per language** and a **Shared text**. The simplest case of a block that varies by language inside a shared list. |
| Nested blocks 01 ... 18                                                     | One document for every way the levels of a nested block editor can vary by language (see below).                                                                                                                                    |

### Nested blocks 01 to 18

A document has an **outer list**, which holds an **outer block**, which has an **inner list**, which holds an **inner block**, which has a **text**.
Every level either varies by language or is shared. The document is named by what is **shared**; everything else varies by language, for example
_Nested blocks 12 - shared: outer list, inner block and text_. Every label in the editor also says whether it varies, for example _Inner list (shared)_.
To reach the text, open the outer block and then the inner block. Every scenario has a block in every level, in every language.

| Scenario | Shared levels                              |
| -------- | ------------------------------------------ |
| 01       | nothing                                    |
| 02       | text                                       |
| 03       | inner block, text                          |
| 04       | inner list                                 |
| 05       | inner list, text                           |
| 06       | inner list, inner block, text              |
| 07       | outer block, inner list                    |
| 08       | outer block, inner list, text              |
| 09       | outer block, inner list, inner block, text |
| 10       | outer list                                 |
| 11       | outer list, text                           |
| 12       | outer list, inner block, text              |
| 13       | outer list, inner list                     |
| 14       | outer list, inner list, text               |
| 15       | outer list, inner list, inner block, text  |
| 16       | outer list, outer block, inner list        |
| 17       | outer list, outer block, inner list, text  |
| 18       | everything                                 |

## The elements

Every document above has an element twin in the **Library** section, with the same rules.

| Element                                                                        | What it is for                                                                          |
| ------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------- |
| Element with text per language and shared text (English / Danish / Vietnamese) | One element with three languages. It has a **Text per language** and a **Shared text**. |
| Element that does not vary by language                                         | An element with one version and one **Text**. It is never restricted by language.       |
| Element with nested blocks 01 ... 18                                           | The same 18 nested block scenarios as the documents, with an element as the owner.      |

## What should happen

- **Name and Text per language** are editable only in a language the user may edit. In any other language they are read-only.
- **Shared text** is editable in every language when the user _can edit shared data_, including languages the user may not edit. When the user _cannot edit shared data_ it is read-only in every language, the default language included.
- **Page that does not vary by language** is always editable.
- **Page with a shared block list** follows the same rules inside the block: open the block in any language. Its **Text per language** is editable only in a language the user may edit. Its **Shared text** is editable in every language when the user _can edit shared data_, and read-only in every language when the user _cannot edit shared data_.
- The language picker of the document and the app language mark a language the user may not edit as **Read-only**, also when the user can edit shared data. Shared text on that language can still be edited by a user who can edit shared data.
- Saving and publishing only offer the languages the user may edit.
- A user who may not edit any language but can edit shared data can still save the shared data on its own, but can publish no language.
- A user who may not edit any language and cannot edit shared data cannot save.

**Blocks** follow the same idea. Block content that belongs to a language is editable only in a language the user may edit.
Shared block content, which is anything that is not inside a list that varies by language, is editable in every language when the user _can edit shared data_.
When the user _cannot edit shared data_, shared block content is read-only. Only the text of the inner block is checked in the table below.

For a user who may edit **Danish only**, the text of the inner block in the **Danish** language:

| Scenarios           | Cannot edit shared data | Can edit shared data |
| ------------------- | ----------------------- | -------------------- |
| 14, 15, 17, 18      | read-only               | editable             |
| All other scenarios | editable                | editable             |

In the **English** language it is read-only for that user, except in scenario 18 (everything is shared), where a user who can edit shared data may edit it.

Per user, the languages in which Name and Text per language are editable:

| User                                           | English | Danish | Vietnamese |
| ---------------------------------------------- | ------- | ------ | ---------- |
| All languages (either shared data access)      | yes     | yes    | yes        |
| English only (either shared data access)       | yes     | no     | no         |
| Danish only (either shared data access)        | no      | yes    | no         |
| English and Danish (either shared data access) | yes     | yes    | no         |
| No languages (either shared data access)       | no      | no     | no         |
