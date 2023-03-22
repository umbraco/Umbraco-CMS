import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUITreePickerStartNodeElement } from './property-editor-ui-tree-picker-start-node.element';
import './property-editor-ui-tree-picker-start-node.element';

export default {
	title: 'Property Editor UIs/Tree Picker Start Node',
	component: 'umb-property-editor-ui-tree-picker-start-node',
	id: 'umb-property-editor-ui-tree-picker-start-node',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUITreePickerStartNodeElement> = () =>
	html`<umb-property-editor-ui-tree-picker-start-node></umb-property-editor-ui-tree-picker-start-node>`;
AAAOverview.storyName = 'Overview';
