import type { Meta, StoryObj } from '@storybook/web-components';
import './input-code-editor.element.js';
import type { UmbInputCodeEditorElement } from './input-code-editor.element.js';

const meta: Meta<UmbInputCodeEditorElement> = {
	title: 'Components/Inputs/Code Editor',
	component: 'umb-input-code-editor',
	args: {
		preview: false,
	},
};

export default meta;
type Story = StoryObj<UmbInputCodeEditorElement>;

export const Overview: Story = {};
