import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIBlockGridStylesheetPickerElement } from './property-editor-ui-block-grid-stylesheet-picker.element';
import './property-editor-ui-block-grid-stylesheet-picker.element';

export default {
	title: 'Property Editor UIs/Block Grid Stylesheet Picker',
	component: 'umb-property-editor-ui-block-grid-stylesheet-picker',
	id: 'umb-property-editor-ui-block-grid-stylesheet-picker',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIBlockGridStylesheetPickerElement> = () =>
	html`<umb-property-editor-ui-block-grid-stylesheet-picker></umb-property-editor-ui-block-grid-stylesheet-picker>`;
AAAOverview.storyName = 'Overview';
