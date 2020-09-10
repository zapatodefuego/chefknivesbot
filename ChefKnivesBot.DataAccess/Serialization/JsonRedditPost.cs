namespace ChefKnivesBot.DataAccess.Serialization
{
    public class RedditPostQueryResponse
    {
        public string kind { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public string modhash { get; set; }
        public int dist { get; set; }
        public Post[] children { get; set; }
        public string after { get; set; }
        public object before { get; set; }
    }

    public class Post
    {
        public string kind { get; set; }
        public RedditPostExpanded data { get; set; }
    }

    public class RedditPostExpanded
    {
        public object approved_at_utc { get; set; }
        public string subreddit { get; set; }
        public string selftext { get; set; }
        public string author_fullname { get; set; }
        public bool saved { get; set; }
        public object mod_reason_title { get; set; }
        public int gilded { get; set; }
        public bool clicked { get; set; }
        public string title { get; set; }
        public Link_Flair_Richtext[] link_flair_richtext { get; set; }
        public string subreddit_name_prefixed { get; set; }
        public bool hidden { get; set; }
        public int pwls { get; set; }
        public string link_flair_css_class { get; set; }
        public int downs { get; set; }
        public int? thumbnail_height { get; set; }
        public object top_awarded_type { get; set; }
        public bool hide_score { get; set; }
        public string name { get; set; }
        public bool quarantine { get; set; }
        public string link_flair_text_color { get; set; }
        public float upvote_ratio { get; set; }
        public object author_flair_background_color { get; set; }
        public string subreddit_type { get; set; }
        public int ups { get; set; }
        public int total_awards_received { get; set; }
        public Media_Embed media_embed { get; set; }
        public int? thumbnail_width { get; set; }
        public object author_flair_template_id { get; set; }
        public bool is_original_content { get; set; }
        public object[] user_reports { get; set; }
        public Secure_Media secure_media { get; set; }
        public bool is_reddit_media_domain { get; set; }
        public bool is_meta { get; set; }
        public object category { get; set; }
        public Secure_Media_Embed secure_media_embed { get; set; }
        public string link_flair_text { get; set; }
        public bool can_mod_post { get; set; }
        public int score { get; set; }
        public object approved_by { get; set; }
        public bool author_premium { get; set; }
        public string thumbnail { get; set; }
        public object edited { get; set; }
        public object author_flair_css_class { get; set; }
        public object[] author_flair_richtext { get; set; }
        public Gildings gildings { get; set; }
        public object content_categories { get; set; }
        public bool is_self { get; set; }
        public object mod_note { get; set; }

        // kept as string to avoid truncation
        public string created { get; set; }
        public string link_flair_type { get; set; }
        public int wls { get; set; }
        public object removed_by_category { get; set; }
        public object banned_by { get; set; }
        public string author_flair_type { get; set; }
        public string domain { get; set; }
        public bool allow_live_comments { get; set; }
        public string selftext_html { get; set; }
        public bool? likes { get; set; }
        public object suggested_sort { get; set; }
        public object banned_at_utc { get; set; }
        public object view_count { get; set; }
        public bool archived { get; set; }
        public bool no_follow { get; set; }
        public bool is_crosspostable { get; set; }
        public bool pinned { get; set; }
        public bool over_18 { get; set; }
        public All_Awardings[] all_awardings { get; set; }
        public object[] awarders { get; set; }
        public bool media_only { get; set; }
        public bool can_gild { get; set; }
        public bool spoiler { get; set; }
        public bool locked { get; set; }
        public object author_flair_text { get; set; }
        public object[] treatment_tags { get; set; }
        public bool visited { get; set; }
        public object removed_by { get; set; }
        public object num_reports { get; set; }
        public object distinguished { get; set; }
        public string subreddit_id { get; set; }
        public object mod_reason_by { get; set; }
        public object removal_reason { get; set; }
        public string link_flair_background_color { get; set; }
        public string id { get; set; }
        public bool is_robot_indexable { get; set; }
        public object report_reasons { get; set; }
        public string author { get; set; }
        public object discussion_type { get; set; }
        public int num_comments { get; set; }
        public bool send_replies { get; set; }
        public string whitelist_status { get; set; }
        public bool contest_mode { get; set; }
        public object[] mod_reports { get; set; }
        public bool author_patreon_flair { get; set; }
        public object author_flair_text_color { get; set; }
        public string permalink { get; set; }
        public string parent_whitelist_status { get; set; }
        public bool stickied { get; set; }
        public string url { get; set; }
        public int subreddit_subscribers { get; set; }

        // kept as string to avoid truncation
        public string created_utc { get; set; }
        public int num_crossposts { get; set; }
        public Media media { get; set; }
        public bool is_video { get; set; }
        public string post_hint { get; set; }
        public string url_overridden_by_dest { get; set; }
        public Preview preview { get; set; }
        public string link_flair_template_id { get; set; }
        public string rte_mode { get; set; }
        public bool author_cakeday { get; set; }
        public bool is_gallery { get; set; }
        public Media_Metadata media_metadata { get; set; }
        public Gallery_Data gallery_data { get; set; }
        public Crosspost_Parent_List[] crosspost_parent_list { get; set; }
        public string crosspost_parent { get; set; }
    }

    public class Media_Embed
    {
        public string content { get; set; }
        public int width { get; set; }
        public bool scrolling { get; set; }
        public int height { get; set; }
    }

    public class Secure_Media
    {
        public Reddit_Video reddit_video { get; set; }
        public string type { get; set; }
        public Oembed oembed { get; set; }
    }

    public class Reddit_Video
    {
        public string fallback_url { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public string scrubber_media_url { get; set; }
        public string dash_url { get; set; }
        public int duration { get; set; }
        public string hls_url { get; set; }
        public bool is_gif { get; set; }
        public string transcoding_status { get; set; }
    }

    public class Oembed
    {
        public string provider_url { get; set; }
        public string version { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public int thumbnail_width { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public string html { get; set; }
        public string author_name { get; set; }
        public string provider_name { get; set; }
        public string thumbnail_url { get; set; }
        public int thumbnail_height { get; set; }
        public string author_url { get; set; }
    }

    public class Media
    {
        public Reddit_Video1 reddit_video { get; set; }
        public string type { get; set; }
        public Oembed1 oembed { get; set; }
    }

    public class Reddit_Video1
    {
        public string fallback_url { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public string scrubber_media_url { get; set; }
        public string dash_url { get; set; }
        public int duration { get; set; }
        public string hls_url { get; set; }
        public bool is_gif { get; set; }
        public string transcoding_status { get; set; }
    }

    public class Oembed1
    {
        public string provider_url { get; set; }
        public string version { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public int thumbnail_width { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public string html { get; set; }
        public string author_name { get; set; }
        public string provider_name { get; set; }
        public string thumbnail_url { get; set; }
        public int thumbnail_height { get; set; }
        public string author_url { get; set; }
    }

    public class Preview
    {
        public Image[] images { get; set; }
        public bool enabled { get; set; }
    }

    public class Image
    {
        public Source source { get; set; }
        public Resolution[] resolutions { get; set; }
        public Variants variants { get; set; }
        public string id { get; set; }
    }

    public class Source
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Variants
    {
    }

    public class Resolution
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Media_Metadata
    {
        public Suyn9kd98lj51 suyn9kd98lj51 { get; set; }
        public Sw4eild98lj51 sw4eild98lj51 { get; set; }
        public Wwl6wnx0vkj51 wwl6wnx0vkj51 { get; set; }
        public Aremllx0vkj51 aremllx0vkj51 { get; set; }
        public _33Naxe6uikj51 _33naxe6uikj51 { get; set; }
        public A5zycf6uikj51 a5zycf6uikj51 { get; set; }
        public Balzqf6uikj51 balzqf6uikj51 { get; set; }
        public Kszqgb6uikj51 kszqgb6uikj51 { get; set; }
        public _4Strfsfangj51 _4strfsfangj51 { get; set; }
        public Etfls4ondgj51 etfls4ondgj51 { get; set; }
        public _0Y9gp3ondgj51 _0y9gp3ondgj51 { get; set; }
        public U5uzxxhcxej51 u5uzxxhcxej51 { get; set; }
        public Kkwp87vdxej51 kkwp87vdxej51 { get; set; }
        public V72mxgwexej51 v72mxgwexej51 { get; set; }
        public _4Pjikr6bxej51 _4pjikr6bxej51 { get; set; }
        public Ylxqvkwuscj51 ylxqvkwuscj51 { get; set; }
        public Ysgvdkwuscj51 ysgvdkwuscj51 { get; set; }
        public Ibrf2mwuscj51 ibrf2mwuscj51 { get; set; }
        public Qayo4mwuscj51 qayo4mwuscj51 { get; set; }
        public Cq9n170ay9j51 cq9n170ay9j51 { get; set; }
        public Kqqqzh4ay9j51 kqqqzh4ay9j51 { get; set; }
        public _7Gm12hn249j51 _7gm12hn249j51 { get; set; }
        public Yeo0chn249j51 yeo0chn249j51 { get; set; }
        public _7Arn9hn249j51 _7arn9hn249j51 { get; set; }
        public _1Fwimhn249j51 _1fwimhn249j51 { get; set; }
        public _2Zsipbc9z7j51 _2zsipbc9z7j51 { get; set; }
        public N8rwibc9z7j51 n8rwibc9z7j51 { get; set; }
        public W4zbsbc9z7j51 w4zbsbc9z7j51 { get; set; }
        public Kezwjbc9z7j51 kezwjbc9z7j51 { get; set; }
        public Ft5i6ec9z7j51 ft5i6ec9z7j51 { get; set; }
        public A6p4h9mu87j51 a6p4h9mu87j51 { get; set; }
        public Dtazhwcy87j51 dtazhwcy87j51 { get; set; }
        public Apjw7vf397j51 apjw7vf397j51 { get; set; }
    }

    public class Suyn9kd98lj51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P[] p { get; set; }
        public S s { get; set; }
        public string id { get; set; }
    }

    public class S
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Sw4eild98lj51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P1[] p { get; set; }
        public S1 s { get; set; }
        public string id { get; set; }
    }

    public class S1
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P1
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Wwl6wnx0vkj51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P2[] p { get; set; }
        public S2 s { get; set; }
        public string id { get; set; }
    }

    public class S2
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P2
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Aremllx0vkj51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P3[] p { get; set; }
        public S3 s { get; set; }
        public string id { get; set; }
    }

    public class S3
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P3
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class _33Naxe6uikj51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P4[] p { get; set; }
        public S4 s { get; set; }
        public string id { get; set; }
    }

    public class S4
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P4
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class A5zycf6uikj51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P5[] p { get; set; }
        public S5 s { get; set; }
        public string id { get; set; }
    }

    public class S5
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P5
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Balzqf6uikj51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P6[] p { get; set; }
        public S6 s { get; set; }
        public string id { get; set; }
    }

    public class S6
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P6
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Kszqgb6uikj51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P7[] p { get; set; }
        public S7 s { get; set; }
        public string id { get; set; }
    }

    public class S7
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P7
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class _4Strfsfangj51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P8[] p { get; set; }
        public S8 s { get; set; }
        public string id { get; set; }
    }

    public class S8
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P8
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Etfls4ondgj51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P9[] p { get; set; }
        public S9 s { get; set; }
        public string id { get; set; }
    }

    public class S9
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P9
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class _0Y9gp3ondgj51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P10[] p { get; set; }
        public S10 s { get; set; }
        public string id { get; set; }
    }

    public class S10
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P10
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class U5uzxxhcxej51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P11[] p { get; set; }
        public S11 s { get; set; }
        public string id { get; set; }
    }

    public class S11
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P11
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Kkwp87vdxej51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P12[] p { get; set; }
        public S12 s { get; set; }
        public string id { get; set; }
    }

    public class S12
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P12
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class V72mxgwexej51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P13[] p { get; set; }
        public S13 s { get; set; }
        public string id { get; set; }
    }

    public class S13
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P13
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class _4Pjikr6bxej51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P14[] p { get; set; }
        public S14 s { get; set; }
        public string id { get; set; }
    }

    public class S14
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P14
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Ylxqvkwuscj51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P15[] p { get; set; }
        public S15 s { get; set; }
        public string id { get; set; }
    }

    public class S15
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P15
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Ysgvdkwuscj51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P16[] p { get; set; }
        public S16 s { get; set; }
        public string id { get; set; }
    }

    public class S16
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P16
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Ibrf2mwuscj51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P17[] p { get; set; }
        public S17 s { get; set; }
        public string id { get; set; }
    }

    public class S17
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P17
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Qayo4mwuscj51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P18[] p { get; set; }
        public S18 s { get; set; }
        public string id { get; set; }
    }

    public class S18
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P18
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Cq9n170ay9j51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P19[] p { get; set; }
        public S19 s { get; set; }
        public string id { get; set; }
    }

    public class S19
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P19
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Kqqqzh4ay9j51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P20[] p { get; set; }
        public S20 s { get; set; }
        public string id { get; set; }
    }

    public class S20
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P20
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class _7Gm12hn249j51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P21[] p { get; set; }
        public S21 s { get; set; }
        public string id { get; set; }
    }

    public class S21
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P21
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Yeo0chn249j51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P22[] p { get; set; }
        public S22 s { get; set; }
        public string id { get; set; }
    }

    public class S22
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P22
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class _7Arn9hn249j51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P23[] p { get; set; }
        public S23 s { get; set; }
        public string id { get; set; }
    }

    public class S23
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P23
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class _1Fwimhn249j51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P24[] p { get; set; }
        public S24 s { get; set; }
        public string id { get; set; }
    }

    public class S24
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P24
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class _2Zsipbc9z7j51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P25[] p { get; set; }
        public S25 s { get; set; }
        public string id { get; set; }
    }

    public class S25
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P25
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class N8rwibc9z7j51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P26[] p { get; set; }
        public S26 s { get; set; }
        public string id { get; set; }
    }

    public class S26
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P26
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class W4zbsbc9z7j51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P27[] p { get; set; }
        public S27 s { get; set; }
        public string id { get; set; }
    }

    public class S27
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P27
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Kezwjbc9z7j51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P28[] p { get; set; }
        public S28 s { get; set; }
        public string id { get; set; }
    }

    public class S28
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P28
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Ft5i6ec9z7j51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P29[] p { get; set; }
        public S29 s { get; set; }
        public string id { get; set; }
    }

    public class S29
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P29
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class A6p4h9mu87j51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P30[] p { get; set; }
        public S30 s { get; set; }
        public string id { get; set; }
    }

    public class S30
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P30
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Dtazhwcy87j51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P31[] p { get; set; }
        public S31 s { get; set; }
        public string id { get; set; }
    }

    public class S31
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P31
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Apjw7vf397j51
    {
        public string status { get; set; }
        public string e { get; set; }
        public string m { get; set; }
        public P32[] p { get; set; }
        public S32 s { get; set; }
        public string id { get; set; }
    }

    public class S32
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class P32
    {
        public int y { get; set; }
        public int x { get; set; }
        public string u { get; set; }
    }

    public class Gallery_Data
    {
        public Item[] items { get; set; }
    }

    public class Item
    {
        public string media_id { get; set; }
        public int id { get; set; }
        public string caption { get; set; }
    }

    public class Link_Flair_Richtext
    {
        public string e { get; set; }
        public string t { get; set; }
    }

    public class All_Awardings
    {
        public object giver_coin_reward { get; set; }
        public object subreddit_id { get; set; }
        public bool is_new { get; set; }
        public int days_of_drip_extension { get; set; }
        public int coin_price { get; set; }
        public object resized_tier_icons { get; set; }
        public string id { get; set; }
        public object penny_donate { get; set; }
        public string award_sub_type { get; set; }
        public int coin_reward { get; set; }
        public string icon_url { get; set; }
        public int days_of_premium { get; set; }
        public object tiers_by_required_awardings { get; set; }
        public Resized_Icons[] resized_icons { get; set; }
        public int icon_width { get; set; }
        public int static_icon_width { get; set; }
        public object start_date { get; set; }
        public bool is_enabled { get; set; }
        public object awardings_required_to_grant_benefits { get; set; }
        public string description { get; set; }
        public object end_date { get; set; }
        public int subreddit_coin_reward { get; set; }
        public int count { get; set; }
        public int static_icon_height { get; set; }
        public string name { get; set; }
        public Resized_Static_Icons[] resized_static_icons { get; set; }
        public object icon_format { get; set; }
        public int icon_height { get; set; }
        public object penny_price { get; set; }
        public string award_type { get; set; }
        public string static_icon_url { get; set; }
    }

    public class Resized_Icons
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Resized_Static_Icons
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Crosspost_Parent_List
    {
        public object approved_at_utc { get; set; }
        public string subreddit { get; set; }
        public string selftext { get; set; }
        public string author_fullname { get; set; }
        public bool saved { get; set; }
        public object mod_reason_title { get; set; }
        public int gilded { get; set; }
        public bool clicked { get; set; }
        public string title { get; set; }
        public object[] link_flair_richtext { get; set; }
        public string subreddit_name_prefixed { get; set; }
        public bool hidden { get; set; }
        public int pwls { get; set; }
        public object link_flair_css_class { get; set; }
        public int downs { get; set; }
        public object thumbnail_height { get; set; }
        public object top_awarded_type { get; set; }
        public bool hide_score { get; set; }
        public string name { get; set; }
        public bool quarantine { get; set; }
        public string link_flair_text_color { get; set; }
        public float upvote_ratio { get; set; }
        public object author_flair_background_color { get; set; }
        public string subreddit_type { get; set; }
        public int ups { get; set; }
        public int total_awards_received { get; set; }
        public Media_Embed1 media_embed { get; set; }
        public object thumbnail_width { get; set; }
        public object author_flair_template_id { get; set; }
        public bool is_original_content { get; set; }
        public object[] user_reports { get; set; }
        public object secure_media { get; set; }
        public bool is_reddit_media_domain { get; set; }
        public bool is_meta { get; set; }
        public object category { get; set; }
        public Secure_Media_Embed1 secure_media_embed { get; set; }
        public object link_flair_text { get; set; }
        public bool can_mod_post { get; set; }
        public int score { get; set; }
        public object approved_by { get; set; }
        public bool author_premium { get; set; }
        public string thumbnail { get; set; }

        // kept as string to avoid truncation
        public string edited { get; set; }
        public object author_flair_css_class { get; set; }
        public object[] author_flair_richtext { get; set; }
        public Gildings1 gildings { get; set; }
        public object content_categories { get; set; }
        public bool is_self { get; set; }
        public object mod_note { get; set; }
        
        // kept as string to avoid truncation
        public string created { get; set; }
        public string link_flair_type { get; set; }
        public int wls { get; set; }
        public object removed_by_category { get; set; }
        public object banned_by { get; set; }
        public string author_flair_type { get; set; }
        public string domain { get; set; }
        public bool allow_live_comments { get; set; }
        public string selftext_html { get; set; }
        public object likes { get; set; }
        public object suggested_sort { get; set; }
        public object banned_at_utc { get; set; }
        public object view_count { get; set; }
        public bool archived { get; set; }
        public bool no_follow { get; set; }
        public bool is_crosspostable { get; set; }
        public bool pinned { get; set; }
        public bool over_18 { get; set; }
        public object[] all_awardings { get; set; }
        public object[] awarders { get; set; }
        public bool media_only { get; set; }
        public bool can_gild { get; set; }
        public bool spoiler { get; set; }
        public bool locked { get; set; }
        public object author_flair_text { get; set; }
        public object[] treatment_tags { get; set; }
        public bool visited { get; set; }
        public object removed_by { get; set; }
        public object num_reports { get; set; }
        public object distinguished { get; set; }
        public string subreddit_id { get; set; }
        public object mod_reason_by { get; set; }
        public object removal_reason { get; set; }
        public string link_flair_background_color { get; set; }
        public string id { get; set; }
        public bool is_robot_indexable { get; set; }
        public object report_reasons { get; set; }
        public string author { get; set; }
        public object discussion_type { get; set; }
        public int num_comments { get; set; }
        public bool send_replies { get; set; }
        public string whitelist_status { get; set; }
        public bool contest_mode { get; set; }
        public object[] mod_reports { get; set; }
        public bool author_patreon_flair { get; set; }
        public object author_flair_text_color { get; set; }
        public string permalink { get; set; }
        public string parent_whitelist_status { get; set; }
        public bool stickied { get; set; }
        public string url { get; set; }
        public int subreddit_subscribers { get; set; }
        public float created_utc { get; set; }
        public int num_crossposts { get; set; }
        public object media { get; set; }
        public bool is_video { get; set; }
    }

    public class Media_Embed1
    {
    }

    public class Secure_Media_Embed1
    {
    }

    public class Gildings1
    {
    }

}
