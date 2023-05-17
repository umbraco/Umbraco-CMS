import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIMemberPickerElement } from './property-editor-ui-member-picker.element';
import './property-editor-ui-member-picker.element';

export default {
	title: 'Property Editor UIs/Member Picker',
	component: 'umb-property-editor-ui-member-picker',
	id: 'umb-property-editor-ui-member-picker',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIMemberPickerElement> = () =>
	html`<umb-property-editor-ui-member-picker></umb-property-editor-ui-member-picker>`;
AAAOverview.storyName = 'Overview';
