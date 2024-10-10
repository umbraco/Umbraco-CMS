import type { UmbPropertyEditorUIUserPickerElement } from './property-editor-ui-user-picker.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-user-picker.element.js';

export default {
	title: 'Property Editor UIs/User Picker',
	component: 'umb-property-editor-ui-user-picker',
	id: 'umb-property-editor-ui-user-picker',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIUserPickerElement> = () =>
	html`<umb-property-editor-ui-user-picker></umb-property-editor-ui-user-picker>`;
AAAOverview.storyName = 'Overview';
