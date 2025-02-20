import type { UmbPropertyEditorUIMemberPickerElement } from './property-editor-ui-member-picker.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-member-picker.element.js';

export default {
	title: 'Property Editor UIs/Member Picker',
	component: 'umb-property-editor-ui-member-picker',
	id: 'umb-property-editor-ui-member-picker',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIMemberPickerElement> = () =>
	html`<umb-property-editor-ui-member-picker></umb-property-editor-ui-member-picker>`;
AAAOverview.storyName = 'Overview';
