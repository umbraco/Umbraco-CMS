import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUITreePickerStartNodeElement } from './property-editor-ui-tree-picker-start-node.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-tree-picker-start-node.element.js';

export default {
	title: 'Property Editor UIs/Tree Picker Start Node',
	component: 'umb-property-editor-ui-tree-picker-start-node',
	id: 'umb-property-editor-ui-tree-picker-start-node',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUITreePickerStartNodeElement> = () =>
	html`<umb-property-editor-ui-tree-picker-start-node></umb-property-editor-ui-tree-picker-start-node>`;
AAAOverview.storyName = 'Overview';
