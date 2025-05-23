import type { UmbPropertyEditorConfigElement } from './property-editor-config.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-config.element.js';

export default {
	title: 'Property Editors/Shared/Property Editor Config',
	component: 'umb-property-editor-config',
	id: 'umb-property-editor-config',
} as Meta;

const data = [
	{
		alias: 'maxChars',
		value: 100,
	},
];

export const AAAOverview: StoryFn<UmbPropertyEditorConfigElement> = () =>
	html`<umb-property-editor-config
		property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"
		.data="${data}"></umb-property-editor-config>`;
AAAOverview.storyName = 'Overview';
