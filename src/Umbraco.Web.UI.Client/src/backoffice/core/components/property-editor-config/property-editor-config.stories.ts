import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorConfigElement } from './property-editor-config.element';
import './property-editor-config.element';

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

export const AAAOverview: Story<UmbPropertyEditorConfigElement> = () =>
	html`<umb-property-editor-config
		property-editor-ui-alias="Umb.PropertyEditorUI.TextBox"
		.data="${data}"></umb-property-editor-config>`;
AAAOverview.storyName = 'Overview';
