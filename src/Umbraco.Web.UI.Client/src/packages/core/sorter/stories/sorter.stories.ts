import { Meta, StoryObj } from '@storybook/web-components';
import { html } from 'lit';
import type UmbTestSorterControllerElement from './test-sorter-controller.element.js';
import './test-sorter-controller.element.js';

const meta: Meta<UmbTestSorterControllerElement> = {
	title: 'API/Drag and Drop/Sorter',
	component: 'test-my-sorter-controller',
	decorators: [
		(Story) => {
			return html`<div
				style="margin:2rem auto; width: 50%; min-height: 350px; padding: 20px; box-sizing: border-box; background:white;">
				<p>
					<strong>Drag and drop the items to sort them.</strong>
				</p>
				${Story()}
			</div>`;
		},
	],
};

export default meta;

type Story = StoryObj<UmbTestSorterControllerElement>;

export const Default: Story = {};
