import type { Meta, StoryObj } from '@storybook/web-components';
import './input-upload-field.element.js';
import type { UmbInputUploadFieldElement } from './input-upload-field.element.js';

const meta: Meta<UmbInputUploadFieldElement> = {
	title: 'Components/Inputs/Upload Field',
	component: 'umb-input-upload-field',
};

export default meta;
type Story = StoryObj<UmbInputUploadFieldElement>;

export const Overview: Story = {
	args: {},
};
