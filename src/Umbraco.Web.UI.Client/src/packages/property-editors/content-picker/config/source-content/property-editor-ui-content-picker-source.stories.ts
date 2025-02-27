import type { UmbPropertyEditorUIContentPickerSourceElement } from './property-editor-ui-content-picker-source.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-content-picker-source.element.js';

export default {
	title: 'Property Editor UIs/Content Picker Start Node',
	component: 'umb-property-editor-ui-content-picker-source',
	id: 'umb-property-editor-ui-content-picker-source',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIContentPickerSourceElement> = () =>
	html`<umb-property-editor-ui-content-picker-source></umb-property-editor-ui-content-picker-source>`;
AAAOverview.storyName = 'Overview';
