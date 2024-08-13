# Backoffice Validation System

The validation system works around a system of Validation Messages, provided via Validation Contexts and connected to the application via Validators.

The system both supports handling front-end validation, server-validation and other things can as well be hooked into it.

## Validation Context

The core of the system is a Validation Context, this holds the messages and more.

### Validation Messages

A Validation message consist of a type, path and body. This typically looks like this:

```
{
	type: "client",
	path: "$.values[?(@.alias = 'my-property-alias')].value",
	message: "Must contain at least 3 words"
}
```

Because each validation issue is presented in the Validation Context as a Message, its existence will be available for anyone to observe.
One benefit of this is that Elements that are removed from screen can still have their validation messages preventing the submission of a dataset.
As well Tabs and other navigation can use this to be highlighted, so the user can be guide to the location.

#### Path

The Path, points to the location of the model that the message is concerning.

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
"$.values.[?(@.alias = 'my-alias')].value"
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

### Server Model Validator
