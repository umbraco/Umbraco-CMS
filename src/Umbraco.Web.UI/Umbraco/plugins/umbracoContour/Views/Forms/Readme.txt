Custom markup for your Contour forms
----------------------------
It's possible to update the default view for the Contour form and fieldtypes, 
making it possible to have complete control over your form markup.
You can do this for al your forms or for a single form.

To do this for all your forms simply update the default views:
~\umbraco\plugins\umbracoContour\Views\Form.cshtml (view for the form, including page name, fieldset, legend, field container)
~\umbraco\plugins\umbracoContour\Views\FieldType.*.cshtml (view for a specific field type like textfield, datepicker, ...)

If you want to do this for a specific form you'll need to create the following folder:
~\umbraco\plugins\umbracoContour\Views\Forms\{FormId}\ (FormId needs to be an existing form id, you can view the id of the form on the settings tab of the form designer)

As an example if your form id is 85514c04-e188-43d0-9246-98b34069750c then you can overwrite the form view by adding the Form.cshtml file to the directory
First copying the default one and then making your changes is the best way to get started
~\umbraco\plugins\umbracoContour\Views\Forms\85514c04-e188-43d0-9246-98b34069750c/Form.cshtml

You can also overwrite views for 1 or more fieldtypes by adding the views to the folder (again if you first copy the default one and then make your changes...)
~\umbraco\plugins\umbracoContour\Views\Forms\85514c04-e188-43d0-9246-98b34069750c\Fieldtype.Textfield.cshtml

A final option is to overwrite the view for a specific field on a form (if you want to target a specific field but not all fields of this type)
~\umbraco\plugins\umbracoContour\Views\Forms\85514c04-e188-43d0-9246-98b34069750c\FieldNameWithoutSpaces.cshtml





