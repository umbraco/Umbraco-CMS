import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUIBlockGridStylesheetPickerElement } from './property-editor-ui-block-grid-stylesheet-picker.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-block-grid-stylesheet-picker.element.js';

export default {
	title: 'Property Editor UIs/Block Grid Stylesheet Picker',
	component: 'umb-property-editor-ui-block-grid-stylesheet-picker',
	id: 'umb-property-editor-ui-block-grid-stylesheet-picker',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIBlockGridStylesheetPickerElement> = () =>
	html`<umb-property-editor-ui-block-grid-stylesheet-picker></umb-property-editor-ui-block-grid-stylesheet-picker>`;
AAAOverview.storyName = 'Overview';
