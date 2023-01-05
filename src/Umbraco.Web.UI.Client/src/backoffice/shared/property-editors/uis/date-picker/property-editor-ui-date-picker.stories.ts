import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorUIDatePickerElement } from './property-editor-ui-date-picker.element';
import './property-editor-ui-date-picker.element';

export default {
	title: 'Property Editor UIs/Date Picker',
	component: 'umb-property-editor-ui-date-picker',
	id: 'umb-property-editor-ui-date-picker',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIDatePickerElement> = () =>
	html`<umb-property-editor-ui-date-picker></umb-property-editor-ui-date-picker>`;
AAAOverview.storyName = 'Overview';
