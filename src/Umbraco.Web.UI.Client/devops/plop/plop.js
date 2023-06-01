import { kebabCase, camelCase, startCase } from 'lodash-es';
const pascalCase = (str) => startCase(str).replace(/ /g, '');

const tagNamePrefix = 'umb-';
const classNamePrefix = 'Umb';

const extensions = [
	{
		type: 'propertyEditorUi',
		path: '../../src/backoffice/shared/property-editors/uis',
		templatePath: './templates/property-editor-ui',
	},
];

export default function (plop) {
	plop.setHelper('className', (type, name) => classNamePrefix + pascalCase(type) + pascalCase(name) + 'Element');
	plop.setHelper('displayName', (name) => startCase(camelCase(name)));
	plop.setHelper('extensionPath', (type, name) => extensions.find((e) => e.type === type).path + '/' + name);
	plop.setHelper('extensionTemplatePath', (type) => extensions.find((e) => e.type === type).templatePath);
	plop.setHelper('extensionFilename', (type, name) => kebabCase(type) + '-' + kebabCase(name));
	plop.setHelper('extensionTagName', (type, name) => tagNamePrefix + kebabCase(type) + '-' + kebabCase(name));

	plop.setGenerator('component', {
		description: 'application controller logic',
		prompts: [
			{
				type: 'list',
				name: 'extensionType',
				message: 'Select extension type',
				choices: extensions.map((e) => e.type),
			},
			{
				type: 'input',
				name: 'name',
				message: 'Enter extension name (i.e. color-picker)',
				validate: (answer) => {
					if (answer.length < 1) {
						return 'Please enter a name for the extension';
					} else return true;
				},
				// Convert the input into kebab case if not provided as such and strip prefix
				filter: (response) => kebabCase(response.replace(/^umb-/, '')),
			},
		],
		actions: [
			{
				type: 'add',
				path: '{{ extensionPath extensionType name }}/{{extensionFilename extensionType name }}.element.ts',
				templateFile: '{{extensionTemplatePath extensionType}}/element.ts.hbs',
			},
			{
				type: 'add',
				path: '{{ extensionPath extensionType name }}/{{ extensionFilename extensionType name }}.test.ts',
				templateFile: '{{extensionTemplatePath extensionType}}/test.ts.hbs',
			},
			{
				type: 'add',
				path: '{{ extensionPath extensionType name }}/{{ extensionFilename extensionType name }}.stories.ts',
				templateFile: '{{extensionTemplatePath extensionType}}/stories.ts.hbs',
			},
		],
	});
}
