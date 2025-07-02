import type { Meta, StoryObj } from '@storybook/web-components-vite';
import './input-document.element.js';
import type { UmbInputDocumentElement } from './input-document.element.js';

const meta: Meta<UmbInputDocumentElement> = {
	title: 'Silo/Document/Components/Input Document',
	component: 'umb-input-document',
};

export default meta;
type Story = StoryObj<UmbInputDocumentElement>;
export const Overview: Story = {
	args: {},
};
