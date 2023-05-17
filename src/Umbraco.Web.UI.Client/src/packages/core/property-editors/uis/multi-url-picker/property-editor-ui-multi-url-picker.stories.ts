import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIMultiUrlPickerElement } from './property-editor-ui-multi-url-picker.element';
import './property-editor-ui-multi-url-picker.element';

export default {
	title: 'Property Editor UIs/Multi Url Picker',
	component: 'umb-property-editor-ui-multi-url-picker',
	id: 'umb-property-editor-ui-multi-url-picker',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIMultiUrlPickerElement> = () =>
	html`<umb-property-editor-ui-multi-url-picker></umb-property-editor-ui-multi-url-picker>`;
AAAOverview.storyName = 'Overview';
