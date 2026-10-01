import { escapeMarkup, rteInlineBlockTag } from './document-values.js';

const heading = (text: string) => `<h3>${escapeMarkup(text)}</h3>`;
const paragraph = (text: string) => `<p>${escapeMarkup(text)}</p>`;
const list = (items: Array<string>) =>
	`<ul>${items.map((item) => `<li><p>${escapeMarkup(item)}</p></li>`).join('')}</ul>`;

export const BABY_SIZES_TEASER =
	'A size-by-size guide to baby clothes in centimetres, from 50 to 92, with tips on measuring and what to expect at every stage.';

/** The two keys are the Organic Cotton Long-Sleeve Bodysuit teaser and the Basic Organic T-shirt teaser, in that order. */
export const babySizesMarkup = ([bodysuitKey, basicKey]: Array<string>) =>
	[
		'<h2>Baby Sizes Explained: From 50 to 92 cm</h2>',
		paragraph(
			'Shopping for a baby can feel like decoding a secret language. One brand says 0–3 months, another says size 56, and a third simply says newborn. In Denmark and most of Europe, baby clothes are sized by height in centimetres, which is a much more reliable guide than age, because babies grow at their own pace.',
		),
		paragraph(
			'This guide walks through every size from 50 to 92 centimetres: roughly how old your baby will be, what to look for at each stage, and how to avoid buying clothes that are outgrown before they have been worn.',
		),
		heading('Why we size by centimetres'),
		paragraph(
			'A size 62 is made for a baby who is about 62 centimetres tall, whether that baby is three months old or five. Height tells you far more about fit than age does, which is why we list the measurement on every product page. The ages below are only a rough guide.',
		),
		heading('Sizes 50 and 56: the newborn stage'),
		paragraph(
			'Size 50 fits most newborns, and size 56 takes over after the first month or two. At this stage, softness and ease are everything. Look for gentle fabrics, flat seams that will not press on tender skin, and openings that are easy to manage during a sleepy 3 am change. Wrap fronts, envelope necklines and snap fastenings all make life simpler.',
		),
		paragraph(
			'Do not buy too much in these sizes. Many babies skip size 50 completely and move straight into 56, and growth in the first weeks can be surprisingly fast.',
		),
		heading('Sizes 62 and 68: two to six months'),
		paragraph(
			'Between roughly two and six months, babies stretch out, start to lift their heads and begin reaching for everything within range. Sizes 62 and 68 are the workhorses of this period. Bodysuits with a little stretch work best, because they stay put when your baby wriggles, and a generous neckline makes it easier to dress a baby who is starting to resist lying still.',
		),
		heading('Sizes 74 and 80: sitting, rolling and crawling'),
		paragraph(
			'From about six to twelve months, life becomes much more active. Babies sit up, roll over and start to crawl, so knees, elbows and seat take a lot of wear. Choose soft, durable fabrics that can handle regular washing, and look for trousers with a little room in the waist and an elastic or adjustable fit that moves with a growing body.',
		),
		heading('Sizes 86 and 92: the move towards toddlerhood'),
		paragraph(
			'Sizes 86 and 92 cover roughly twelve to twenty-four months, when many babies take their first steps. Fit becomes more about freedom of movement, and the wardrobe often shifts from bodysuits to simple tops and trousers. A good long-sleeve bodysuit still works well as a base layer, and a few plain T-shirts are easy to mix and match with everything else.',
		),
		`<p>${rteInlineBlockTag(bodysuitKey)}${rteInlineBlockTag(basicKey)}</p>`,
		heading('How to measure your baby'),
		paragraph(
			'If you are unsure between two sizes, a quick measurement settles it. It is easiest with a second pair of hands and a calm moment, for example after a nap.',
		),
		list([
			'Lay your baby on their back on a flat surface, with their legs relaxed and straight.',
			'Measure from the top of the head to the heel, using a soft tape measure.',
			'Compare the result with the size chart on the product page, and choose the size that matches or is the next one up.',
		]),
		heading('Between sizes and growth spurts'),
		paragraph(
			'Babies do not grow in a straight line. They can stay the same size for weeks and then shoot up almost overnight. If your baby falls between two sizes, choose the bigger one. A little room is more comfortable than a snug fit, and sleeves and legs can be folded up for a few weeks.',
		),
		paragraph(
			'Fit also depends on the garment. A close-fitting bodysuit will feel smaller than a loose sweater in the same size, and clothes in natural fibres can shrink slightly in the first wash. If you are buying ahead, the care label on each product tells you how to keep clothes in shape.',
		),
		heading('Buying for the season'),
		paragraph(
			'When you are buying ahead, think about the weather when your baby will actually wear the size. A baby born in autumn will be in size 62 and 68 through the winter, so thicker layers are worth buying in those sizes, while a spring baby will need lighter clothes at the same stage.',
		),
		paragraph(
			'Above all, keep it simple. A handful of well-fitting basics that wash well will serve you better than a wardrobe full of tiny outfits, and the sizes will be handed on to the next baby in good shape.',
		),
	].join('');
