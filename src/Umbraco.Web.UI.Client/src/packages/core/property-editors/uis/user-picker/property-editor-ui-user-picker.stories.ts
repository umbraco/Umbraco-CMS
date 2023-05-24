import { Meta, Story } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import type { UmbPropertyEditorUIUserPickerElement } from './property-editor-ui-user-picker.element.js';
import './property-editor-ui-user-picker.element';

export default {
	title: 'Property Editor UIs/User Picker',
	component: 'umb-property-editor-ui-user-picker',
	id: 'umb-property-editor-ui-user-picker',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIUserPickerElement> = () =>
	html`<umb-property-editor-ui-user-picker></umb-property-editor-ui-user-picker>`;
AAAOverview.storyName = 'Overview';
