import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUITreePickerSourcePickerElement } from './property-editor-ui-tree-picker-source-picker.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-tree-picker-source-picker.element.js';

export default {
	title: 'Property Editor UIs/Tree Picker Start Node',
	component: 'umb-property-editor-ui-tree-picker-source-picker',
	id: 'umb-property-editor-ui-tree-picker-source-pickere',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUITreePickerSourcePickerElement> = () =>
	html`<umb-property-editor-ui-tree-picker-source-picker></umb-property-editor-ui-tree-picker-source-picker>`;
AAAOverview.storyName = 'Overview';
