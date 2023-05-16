import { Meta, StoryObj } from '@storybook/web-components';
import './input-color-picker.element';
import type { UmbInputColorPickerElement } from './input-color-picker.element';

const meta: Meta<UmbInputColorPickerElement> = {
    title: 'Components/Inputs/Color Picker',
    component: 'umb-input-color-picker'
};
  
export default meta;
type Story = StoryObj<UmbInputColorPickerElement>;
  
export const Overview: Story = {
    args: {
        showLabels: true,
        swatches: [
            {
                label: "Red",
                value: "#ff0000"
            },
            {
                label: "Green",
                value: "#00ff00"
            }
        ]
    }
};

export const WithoutLabels: Story = {
    args: {
        showLabels: false,
        swatches: [
            {
                label: "Red",
                value: "#ff0000"
            },
            {
                label: "Green",
                value: "#00ff00"
            }
        ]
    }
};

// TODO: This doesn't check the correct swatch when the value is set
// Perhaps a BUG ?
export const WithValueLabels: Story = {
    args: {
        showLabels: true,
        swatches: [
            {
                label: "Red",
                value: "#ff0000"
            },
            {
                label: "Green",
                value: "#00ff00"
            }
        ],
        value: "#00ff00"
    }
};