import type UmbPropertyEditorUIStylesheetPickerElement from './property-editor-ui-stylesheet-picker.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';

import './property-editor-ui-stylesheet-picker.element.js';

const meta: Meta = {
	title: 'Extension Type/Property Editor UI/Stylesheet Picker',
	component: 'umb-property-editor-ui-stylesheet-picker',
	id: 'umb-property-editor-ui-stylesheet-picker',
	args: {
		value: ['/rte-styles.css'],
	},
};

export default meta;
type Story = StoryObj<UmbPropertyEditorUIStylesheetPickerElement>;

export const Docs: Story = {};
