import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIMediaPickerElement } from './property-editor-ui-media-picker.element';
import './property-editor-ui-media-picker.element';

export default {
	title: 'Property Editor UIs/Media Picker',
	component: 'umb-property-editor-ui-media-picker',
	id: 'umb-property-editor-ui-media-picker',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIMediaPickerElement> = () =>
	html`<umb-property-editor-ui-media-picker></umb-property-editor-ui-media-picker>`;
AAAOverview.storyName = 'Overview';
