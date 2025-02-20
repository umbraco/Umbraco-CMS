# Backoffice Validation System

The validation system works around a system of Validation Messages, provided via Validation Contexts and connected to the application via Validators.

The system both supports handling front-end validation, server-validation and other things can as well be hooked into it.

## Validation Context

The core of the system is a Validation Context, this holds the messages and more.

## Validation Messages

A Validation message consist of a type, path and body. This typically looks like this:

```
{
	type: "client",
	path: "$.values[?(@.alias == 'my-property-alias')].value",
	message: "Must contain at least 3 words"
}
```

Because each validation issue is presented in the Validation Context as a Message, its existence will be available for anyone to observe.
One benefit of this is that Elements that are removed from screen can still have their validation messages preventing the submission of a dataset.
As well Tabs and other navigation can use this to be highlighted, so the user can be guide to the location.

### Path aka. Data Path

The Path also named Data Path, A Path pointing to the related data in the model.
A massage uses this to point to the location in the model that the message is concerned.

The following models headline can be target with this path:

Data:
```
{
	settings: {
		title: 'too short'
	}
}
```

JsonPath:
```
"$.settings.title"
```

The following example shows how we use JsonPath Queries to target entries of an array:

Data:
```
{
	values: [
		{
			alias: 'my-alias',
			value: 'my-value'
		}
	]
}
```

JsonPath:
```
"$.values.[?(@.alias == 'my-alias')].value"
```

Paths are based on JSONPath, using JSON Path Queries when looking up data of an Array. Using Queries enables Paths to not point to specific index, but what makes a entry unique.

Messages are set via Validators, which is the glue between a the context and a validation source.

## Validators

Messages can be set by Validators, a Validator gets assigned to the Validation Context. Making the Context aware about the Validators.

When the validation context is asked to Validate, it can then call the `validate` method on all the Validators.

The Validate method can be async, meaning it can request the server or other way figure out its state before resolving.

We provide a few built in Validators which handles most cases.

### Form Control Validator

This Validator binds a Form Control Element with the Validation Context. When the Form Control becomes Invalid, its Validation Message is appended to the Validation Context.

Notice this one also comes as a Lit Directive called `umbBindToValidation`.

Also notice this does not bind server validation to the Form Control, see `UmbBindServerValidationToFormControl`

### Server Model Validator

This Validator can asks a end-point for validation of the model.

The Server Model Validator stores the data that was send to the server on the Validation Context. This is then later used by Validation Path Translators to convert index based paths to Json Path Queries.

This is needed to allow the user to make changes to the data, without loosing the accuracy of the messages coming from server validation.

## Validation Path Translator

Validation Path Translator translate Message Paths into a format that is independent of the actual current data. But compatible with mutations of that data model.
This enables the user to retrieve validation messages from the server, and then the user can insert more items and still have the validation appearing in the right spots.
This would not be possible with index based paths, which is why we translate those into JSON Path Queries.

Such conversation could be from this path:
```
"$.values.[5].value"
```

To this path:
```
"$.values.[?(@.alias == 'my-alias')].value"
```

Once this path is converted to use Json Path Queries, the Data can be changed. The concerned entry might get another index. Without that affecting the accuracy of the path.

### Late registered Path Translators

Translators can be registered late. This means that a Property Editor that has a complex value structure, can register a Path Translator for its part of the data. Such Translator will appear late because the Property might not be rendered in the users current view, but first when the user navigates there.
This is completely fine, as messages can be partly translated and then enhanced by late coming Path Translators.

This fact enables a property to observe if there is any Message Paths that start with the same path as the Data Path for the Property. In this was a property can know that it contains a Validation Message without the Message Path begin completely translated.



## Binders

Validators represent a component of the Validation to be considered, but it does not represent other messages of its path.
To display messages from a given data-path, a Binder is needed. We bring a few to make this happen:

### UmbBindServerValidationToFormControl

This binder takes a Form Control Element and a data-path.
The Data Path is a JSON Path defining where the data of this input is located in the model sent to the server.

```
	this.#validationMessageBinder = new UmbBindServerValidationToFormControl(
		this,
		this.querySelector('#myInput"),
		"$.values.[?(@.alias == 'my-input-alias')].value",
	);
```

Once the binder is initialized you need to keep it updated with the value your form control represents. Notice we do not recommend using events from the form control to notify about the changes.
Instead observe the value in of your data model.

This example is just a dummy example of how that could look:
```
	this.observe(
		this.#value,
		(value) => {
			this.#validationMessageBinder.value = value;
		},
	);
```
