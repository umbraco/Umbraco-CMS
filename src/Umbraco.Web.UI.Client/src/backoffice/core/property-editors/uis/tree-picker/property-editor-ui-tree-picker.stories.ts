import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUITreePickerElement } from './property-editor-ui-tree-picker.element';
import './property-editor-ui-tree-picker.element';

export default {
	title: 'Property Editor UIs/Tree Picker',
	component: 'umb-property-editor-ui-tree-picker',
	id: 'umb-property-editor-ui-tree-picker',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUITreePickerElement> = () =>
	html`<umb-property-editor-ui-tree-picker></umb-property-editor-ui-tree-picker>`;
AAAOverview.storyName = 'Overview';
