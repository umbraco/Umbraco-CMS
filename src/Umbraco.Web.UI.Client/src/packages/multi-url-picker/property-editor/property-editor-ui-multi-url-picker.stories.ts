import type { UmbPropertyEditorUIMultiUrlPickerElement } from './property-editor-ui-multi-url-picker.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-multi-url-picker.element.js';

export default {
	title: 'Property Editor UIs/Multi Url Picker',
	component: 'umb-property-editor-ui-multi-url-picker',
	id: 'umb-property-editor-ui-multi-url-picker',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIMultiUrlPickerElement> = () =>
	html`<umb-property-editor-ui-multi-url-picker></umb-property-editor-ui-multi-url-picker>`;
AAAOverview.storyName = 'Overview';
