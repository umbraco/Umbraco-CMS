import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorUIIconPickerElement } from './property-editor-ui-icon-picker.element';
import './property-editor-ui-icon-picker.element';
import type { UmbModalLayoutIconPickerElement } from 'src/core/services/modal/layouts/icon-picker/modal-layout-icon-picker.element';

export default {
	title: 'Property Editor UIs/Icon Picker',
	component: 'umb-property-editor-ui-icon-picker',
	id: 'umb-property-editor-ui-icon-picker',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIIconPickerElement> = () =>
	html`<umb-property-editor-ui-icon-picker></umb-property-editor-ui-icon-picker>`;
AAAOverview.storyName = 'Overview';

export const Overview2: Story<UmbModalLayoutIconPickerElement> = () =>
	html`<umb-modal-layout-icon-picker></umb-modal-layout-icon-picker>`;
