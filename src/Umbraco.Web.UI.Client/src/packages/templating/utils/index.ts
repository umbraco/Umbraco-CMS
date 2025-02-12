//Below are a copy of
export const getInsertDictionarySnippet = (nodeName: string) => {
	return `@Umbraco.GetDictionaryValue("${nodeName}")`;
};

export const getInsertPartialSnippet = (nodeName: string) =>
	`@await Html.PartialAsync("${nodeName.replace('.cshtml', '')}")`;

export const getQuerySnippet = (queryExpression: string) => {
	let code = '\n@{\n' + '\tvar selection = ' + queryExpression + ';\n}\n';
	code +=
		'<ul>\n' +
		'\t@foreach (var item in selection)\n' +
		'\t{\n' +
		'\t\t<li>\n' +
		'\t\t\t<a href="@item.Url()">@item.Name()</a>\n' +
		'\t\t</li>\n' +
		'\t}\n' +
		'</ul>\n\n';
	return code;
};

export const getRenderBodySnippet = () => '@RenderBody()';

export const getRenderSectionSnippet = (sectionName: string, isMandatory: boolean) =>
	`@RenderSection("${sectionName}", ${isMandatory})`;

export const getAddSectionSnippet = (sectionName: string) => `@section ${sectionName}
{



}`;

export const getUmbracoFieldSnippet = (field: string, defaultValue: string | null = null, recursive = false) => {
	let fallback = null;

	if (recursive !== false && defaultValue !== null) {
		fallback = 'Fallback.To(Fallback.Ancestors, Fallback.DefaultValue)';
	} else if (recursive !== false) {
		fallback = 'Fallback.ToAncestors';
	} else if (defaultValue !== null) {
		fallback = 'Fallback.ToDefaultValue';
	}

	const value = `${field !== null ? `@Model.Value("${field}"` : ''}${
		fallback !== null ? `, fallback: ${fallback}` : ''
	}${defaultValue !== null ? `, defaultValue: (object)"${defaultValue}"` : ''}${field ? ')' : ')'}`;

	return value;
};
