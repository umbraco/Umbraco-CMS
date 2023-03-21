import { Meta, StoryObj } from '@storybook/web-components';
import './input-radio-button-list.element';
import type { UmbInputRadioButtonListElement } from './input-radio-button-list.element';

const meta: Meta<UmbInputRadioButtonListElement> = {
    title: 'Components/Inputs/Radio Button List',
    component: 'umb-input-radio-button-list'
};
  
export default meta;
type Story = StoryObj<UmbInputRadioButtonListElement>;
  
export const Overview: Story = {
    args: {
        list: [
            {
                key: '1',
                sortOrder: 0,
                value: 'One'
            },
            {
                key: '2',
                sortOrder: 1,
                value: 'Two'
            },
            {
                key: '3',
                sortOrder: 2,
                value: 'Three'
            }
        ]
    }
};

export const WithSelectedValue: Story = {
    args: {
        list: [
            {
                key: '1',
                sortOrder: 0,
                value: 'One'
            },
            {
                key: '2',
                sortOrder: 1,
                value: 'Two'
            },
            {
                key: '3',
                sortOrder: 2,
                value: 'Three'
            }
        ],
        selected: '2',
        value: 'Two'
    }
};

export const SortOrder: Story = {
    args: {
        list: [
            {
                key: '1',
                sortOrder: 4,
                value: 'One'
            },
            {
                key: '2',
                sortOrder: 1,
                value: 'Two'
            },
            {
                key: '3',
                sortOrder: 2,
                value: 'Three'
            }
        ]
    }
};
