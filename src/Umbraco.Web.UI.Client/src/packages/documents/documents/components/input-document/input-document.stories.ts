import type { UmbInputDocumentElement } from './input-document.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';

import './input-document.element.js';

const meta: Meta<UmbInputDocumentElement> = {
	title: 'Entity/Document/Components/Input Document',
	component: 'umb-input-document',
};

export default meta;
type Story = StoryObj<UmbInputDocumentElement>;
export const Docs: Story = {
	args: {},
};
