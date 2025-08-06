import type { Meta, StoryObj } from '@storybook/web-components-vite';
import './input-upload-field.element.js';
import type { UmbInputUploadFieldElement } from './input-upload-field.element.js';

const meta: Meta<UmbInputUploadFieldElement> = {
	title: 'Entity/Media/Components/Input Upload Field',
	component: 'umb-input-upload-field',
};

export default meta;
type Story = StoryObj<UmbInputUploadFieldElement>;

export const Docs: Story = {
	args: {},
};
