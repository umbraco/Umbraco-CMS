import type { Meta, StoryObj } from '@storybook/web-components';
import './input-checkbox-list.element.js';
import type { UmbInputCheckboxListElement } from './input-checkbox-list.element.js';

const meta: Meta<UmbInputCheckboxListElement> = {
	title: 'Components/Inputs/Checkbox List',
	component: 'umb-input-checkbox-list',
};

export default meta;
type Story = StoryObj<UmbInputCheckboxListElement>;

export const Overview: Story = {
	args: {
		list: [
			{
				label: 'Umbraco is awesome?',
				value: 'isAwesome',
				checked: true,
			},
			{
				label: 'Attending CodeGarden?',
				value: 'attendingCodeGarden',
				checked: false,
			},
		],
	},
};
