# Block Element Level Variant

### Notes
- When talking about variant data, we mean language variant
- Segment variants are not taken into account at this moment but can be expected to work in a similar manner

### What is it
When an element document type supports variant data (marked as variant and has a property marked as variant) is used 
inside an invariant property on a variant document type, that property is considered to have element level variant data 
which is a form of partial variant data.

When a property editor supports partial variant data (`IDataEditor.CanMergePartialPropertyValues()`) the 
`ContentEditingService` will run the `IDataEditor.MergeVariantInvariantPropertyValue(...)` method to get a valid 
value according to the rules defined for that propertyEditor

Block Element level variant data is this within all (core) property editors derived from blocks
- Umbraco.BlockList
- Umbraco.BlockGrid
- Umbraco.RichText

Most logic regarding this feature can be found in `BlockValuePropertyValueEditorBase`

### Axioms
1. A `null` value for a property, including element level variation based properties, is a valid value
2. The invariant value holds the structure/representation of the underlying variant values
3. The structure takes precedence over the underlying data

## Editing Data

### Access to invariant data 
- All Languages: The user has access to all languages
- Default Language: The user has access to the language that is defined as the default
- AllowEditInvariantFromNonDefault: Configuration setting

| All Languages | Default Language | AllowEditInvariantFromNonDefault | Can Edit Invariant |
|---------------|------------------|----------------------------------|--------------------|
| True          | Inherits True    | N/A                              | True               |
| False         | True             | N/A                              | True               |
| False         | False            | True                             | True               |
| False         | False            | False                            | False              |


### Rules derived from the axioms
- A user with access to invariant data is allowed to add or remove blocks even if those blocks hold language variant 
data they do not have access to.
- A user without access to invariant data is NOT allowed to add or remove blocks.
- A user can only edit element variant properties for the languages they have access to.
- A user is allowed to clear (set value to `null`) an element level variation as long as they have access to edit invariant data.

## Exposing
When a block is defined on invariant level but a language has not had its variant fields filled in yet, 
the variant version of the block might be empty or considered not ready for publishing. The Expose feature allows 
editors to define in which culture a block is ready to be consumed by the publishing process.

The client currently adds a blocks culture to the expose list when editing for that blocks starts in the culture,
more precisely when inline editor for the block is opened.

From an API perspective you are allowed to add and remove cultures from the expose list as long as you have the permissions to do so 

### Axioms
- Expose data is linked to the same permissions as variant editing
- Variant blocks that are not exposed for a specific culture should not be processed by the publish feature

### Rules derived from the axioms
- Only a user with access to a language should be able to remove or add a block to the expose list for that language
- A block that is not exposed for a given language should not exist in the published value of the document for that language
- A block that is not exposed should not be processed when running validation during publishing or by running the validation separately.
