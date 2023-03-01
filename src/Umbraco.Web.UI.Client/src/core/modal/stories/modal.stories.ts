import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';


export default {
	title: 'API/Modals',
	id: 'umb-modal-service',
	argTypes: {
		modalLayout: {
			control: 'select',
			options: ['Confirm', 'Content Picker', 'Property Editor UI Picker', 'Icon Picker'],
		},
	},
} as Meta;

const Template: Story = (props) => {
	return html` <story-modal-service-example .modalLayout=${props.modalLayout}></story-modal-service-example> `;
};

export const Overview = Template.bind({});
