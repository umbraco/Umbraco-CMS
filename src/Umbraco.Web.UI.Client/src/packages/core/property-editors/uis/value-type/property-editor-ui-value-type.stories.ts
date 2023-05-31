import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUIValueTypeElement } from './property-editor-ui-value-type.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-value-type.element.js';

export default {
	title: 'Property Editor UIs/Value Type',
	component: 'umb-property-editor-ui-value-type',
	id: 'umb-property-editor-ui-value-type',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIValueTypeElement> = () =>
	html`<umb-property-editor-ui-value-type></umb-property-editor-ui-value-type>`;
AAAOverview.storyName = 'Overview';
