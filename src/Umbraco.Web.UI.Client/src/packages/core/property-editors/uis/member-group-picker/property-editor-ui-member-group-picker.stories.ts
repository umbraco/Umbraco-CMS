import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIMemberGroupPickerElement } from './property-editor-ui-member-group-picker.element';
import './property-editor-ui-member-group-picker.element';

export default {
	title: 'Property Editor UIs/Member Group Picker',
	component: 'umb-property-editor-ui-member-group-picker',
	id: 'umb-property-editor-ui-member-group-picker',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIMemberGroupPickerElement> = () =>
	html`<umb-property-editor-ui-member-group-picker></umb-property-editor-ui-member-group-picker>`;
AAAOverview.storyName = 'Overview';
