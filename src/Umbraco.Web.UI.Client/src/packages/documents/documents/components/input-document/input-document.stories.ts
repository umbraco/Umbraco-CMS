import type { Meta, StoryObj } from '@storybook/web-components';
import './input-document.element.js';
import type { UmbInputDocumentElement } from './input-document.element.js';

const meta: Meta<UmbInputDocumentElement> = {
	title: 'Components/Inputs/Document',
	component: 'umb-input-document',
};

export default meta;
type Story = StoryObj<UmbInputDocumentElement>;
export const Overview: Story = {
	args: {},
};
