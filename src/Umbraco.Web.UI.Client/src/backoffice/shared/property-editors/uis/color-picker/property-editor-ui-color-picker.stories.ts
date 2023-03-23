import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIColorPickerElement } from './property-editor-ui-color-picker.element';
import './property-editor-ui-color-picker.element';

export default {
	title: 'Property Editor UIs/Color Picker',
	component: 'umb-property-editor-ui-color-picker',
	id: 'umb-property-editor-ui-color-picker',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIColorPickerElement> = () =>
	html`<umb-property-editor-ui-color-picker></umb-property-editor-ui-color-picker>`;
AAAOverview.storyName = 'Overview';
