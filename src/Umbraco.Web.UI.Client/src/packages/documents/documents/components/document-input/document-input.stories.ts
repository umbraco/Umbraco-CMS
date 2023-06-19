import { Meta, StoryObj } from '@storybook/web-components';
import './document-input.element.js';
import type { UmbDocumentInputElement } from './document-input.element.js';

const meta: Meta<UmbDocumentInputElement> = {
	title: 'Components/Inputs/Document',
	component: 'umb-document-Input',
};

export default meta;
type Story = StoryObj<UmbDocumentInputElement>;

export const Overview: Story = {
	args: {},
};
