import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

export default {
	title: 'API/Modals',
	id: 'umb-modal-context',
	argTypes: {
		modalLayout: {
			control: 'select',
			//options: ['Confirm', 'Content Picker', 'Property Editor UI Picker', 'Icon Picker'],
		},
	},
} as Meta;

const Template: Story = (props) => {
	return html`
		Under construction
		<story-modal-context-example .modalLayout=${props.modalLayout}></story-modal-context-example>
	`;
};

export const Overview = Template.bind({});
