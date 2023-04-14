import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';
import { repeat } from 'lit/directives/repeat.js';
import icons from '../../../../public-assets/icons/icons.json';

export default {
	title: 'API/Icons',
	id: 'umb-icons',
} as Meta;

const Template: Story = () => {
	return html`
		<div
			style="display: grid;
    grid-template-columns: repeat(auto-fill, minmax(80px, 1fr));
    grid-gap: var(--uui-size-layout-2);
    margin: var(--uui-size-layout-2);
    place-items: start;
    justify-content: space-between;">
			${repeat(
				icons,
				(icon) => icon.name,
				(icon) =>
					html` <div
						style="
						display: flex;
						flex-direction: column;
						align-items: center;
						justify-content: center;
						text-align: center;
						width: 100%;
    				height: 100%;">
						<uui-icon style="font-size: 30px; margin-bottom: 9px;" name="${icon.name}"></uui-icon
						><small>${icon.name}</small>
					</div>`
			)}
		</div>
	`;
};

export const Overview = Template.bind({});
