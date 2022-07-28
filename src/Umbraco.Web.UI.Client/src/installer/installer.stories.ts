import { Story, Meta } from '@storybook/web-components';
import { html } from 'lit';
import './installer.element';

export default {
	title: 'Installer/Installer',
} as Meta;

const Template: Story = () => html`<umb-installer></umb-installer>`;

export const Overview = Template.bind({});
