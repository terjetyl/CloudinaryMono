﻿using System;
using System.Collections.Generic;
using CloudinaryMono.Actions;
using NUnit.Framework;

namespace CloudinaryMono.Test
{
    [TestFixture]
    public class ApiTest
    {
        Api api;

        [SetUp]
        public void Init()
        {
            Account account = new Account("testcloud", "1234", "abcd");
            api = new Api(account);
        }

        [Test]
        public void TestSign()
        {
            SortedDictionary<string, object> parameters = new SortedDictionary<string, object>();

            parameters.Add("public_id", "sample");
            parameters.Add("timestamp", "1315060510");

            Assert.AreEqual("c3470533147774275dd37996cc4d0e68fd03cd4f", api.GetSign(parameters));
        }

        [Test]
        public void TestEnumToString()
        {
            // should escape http urls

            TagCommand command = TagCommand.SetExclusive;
            string commandStr = Api.GetCloudinaryParam<TagCommand>(command);
            Assert.AreEqual(commandStr, "set_exclusive");
        }

        [Test]
        public void TestCloudName()
        {
            // should use cloud_name from account

            string uri = api.UrlImgUp.BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/test", uri);
        }

        [Test]
        public void TestCustomCloudName()
        {
            // should allow overriding cloud_name in url

            string uri = api.UrlImgUp.CloudName("test123").BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/test123/image/upload/test", uri);
        }

        [Test]
        public void TestSecureDistribution()
        {
            // should use default secure distribution if secure=TRUE

            string uri = api.UrlImgUp.Secure(true).BuildUrl("test");
            Assert.AreEqual("https://cloudinary-a.akamaihd.net/testcloud/image/upload/test", uri);
        }

        [Test]
        public void TestSecureAkamai()
        {
            // should default to akamai if secure is given with private_cdn and no secure_distribution

            string uri = api.UrlImgUp.Secure(true).PrivateCdn(true).BuildUrl("test");
            Assert.AreEqual("https://cloudinary-a.akamaihd.net/testcloud/image/upload/test", uri);
        }

        [Test]
        public void testSecureNonAkamai()
        {
            // should not add cloud_name if private_cdn and secure non akamai secure_distribution

            string uri = api.UrlImgUp.Secure(true).PrivateCdn(true).SecureDistribution("something.cloudfront.net").BuildUrl("test");
            Assert.AreEqual("https://something.cloudfront.net/image/upload/test", uri);
        }

        [Test]
        public void TestHttpPrivateCdn()
        {
            // should not add cloud_name if private_cdn and not secure

            string uri = api.UrlImgUp.PrivateCdn(true).BuildUrl("test");
            Assert.AreEqual("http://testcloud-res.cloudinary.com/image/upload/test", uri);
        }

        [Test]
        public void TestSecureDistributionOverwrite()
        {
            // should allow overwriting secure distribution if secure=TRUE

            string uri = api.UrlImgUp.Secure().SecureDistribution("something.else.com").BuildUrl("test");
            Assert.AreEqual("https://something.else.com/testcloud/image/upload/test", uri);
        }

        [Test]
        public void TestFormat()
        {
            // should use format from options

            string uri = api.UrlImgUp.Format("jpg").BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/test.jpg", uri);
        }

        [Test]
        public void TestCrop()
        {
            Transformation transformation = new Transformation().Width(100).Height(101);
            string uri = api.UrlImgUp.Transform(transformation).BuildUrl("test");

            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/h_101,w_100/test", uri);
            Assert.AreEqual("101", transformation.HtmlHeight);
            Assert.AreEqual("100", transformation.HtmlWidth);

            transformation = new Transformation().Width(100).Height(101).Crop("crop");
            uri = api.UrlImgUp.Transform(transformation).BuildUrl("test");

            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/c_crop,h_101,w_100/test", uri);
        }

        [Test]
        public void TestTransformations()
        {
            // should use x, y, radius, prefix, gravity and quality from options

            Transformation transformation = new Transformation().X(1).Y(2).Radius(3).Gravity("center").Quality(0.4).Prefix("a");
            string uri = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/g_center,p_a,q_0.4,r_3,x_1,y_2/test", uri);
        }

        [Test]
        public void TestSimpleTransformation()
        {
            // should support named transformation

            Transformation transformation = new Transformation().Named("blip");
            string uri = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/t_blip/test", uri);
        }

        [Test]
        public void TestTransformationArray()
        {
            // should support array of named transformations

            Transformation transformation = new Transformation().Named("blip", "blop");
            string uri = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/t_blip.blop/test", uri);
        }

        [Test]
        public void TestBaseTransformationChain()
        {
            // should support base transformation

            Transformation transformation = new Transformation().X(100).Y(100).Crop("fill").Chain().Crop("crop").Width(100);
            string uri = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("100", transformation.HtmlWidth);
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/c_fill,x_100,y_100/c_crop,w_100/test", uri);
        }

        [Test]
        public void TestEagerTransformationList()
        {
            List<Transformation> list = new List<Transformation>(){
                new EagerTransformation().SetFormat("jpg").Crop("scale").Width(2.0),
                new EagerTransformation(new Transformation().Width(10),new Transformation().Angle(10)),
                new Transformation().Width(20).Height(20)
            };

            ImageUploadParams uploadParams = new ImageUploadParams()
            {
                EagerTransforms = list
            };

            SortedDictionary<string, object> dict = uploadParams.ToParamsDictionary();

            Assert.AreEqual("c_scale,w_2/jpg|w_10/a_10/|h_20,w_20", dict["eager"]);
        }

        [Test]
        public void TestBaseTransformationArray()
        {
            // should support array of base transformations

            Transformation transformation = new Transformation().X(100).Y(100).Width(200).Crop("fill").Chain().Radius(10).Chain().Crop("crop").Width(100);
            string uri = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("100", transformation.HtmlWidth);
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/c_fill,w_200,x_100,y_100/r_10/c_crop,w_100/test", uri);
        }

        [Test]
        public void TestExcludeEmptyTransformation()
        {
            Transformation transformation = new Transformation().Chain().X(100).Y(100).Crop("fill").Chain();
            string uri = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/c_fill,x_100,y_100/test", uri);
        }

        [Test]
        public void TestAction()
        {
            // should use type of action from options

            string uri = api.UrlImgUp.Action("facebook").BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/facebook/test", uri);
        }

        [Test]
        public void TestResourceType()
        {
            // should use resource_type from options

            string uri = api.Url.ResourceType("raw").Action("upload").BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/raw/upload/test", uri);
        }

        [Test]
        public void TestIgnoreHttp()
        {
            // should ignore http links only if type is not given or is asset

            string uri = api.UrlImgUp.BuildUrl("http://test");
            Assert.AreEqual("http://test", uri);
            uri = api.Url.ResourceType("image").Action("asset").BuildUrl("http://test");
            Assert.AreEqual("http://test", uri);
            uri = api.Url.ResourceType("image").Action("fetch").BuildUrl("http://test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/fetch/http://test", uri);
        }

        [Test]
        public void TestFetch()
        {
            // should escape fetch urls

            string uri = api.Url.ResourceType("image").Action("fetch").BuildUrl("http://blah.com/hello?a=b");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/fetch/http://blah.com/hello%3Fa%3Db", uri);
        }

        [Test]
        public void TestCdnName()
        {
            // should support extenal cname

            string uri = api.UrlImgUp.CName("hello.com").BuildUrl("test");
            Assert.AreEqual("http://hello.com/testcloud/image/upload/test", uri);
        }

        [Test]
        public void TestSubDomain()
        {
            // should support extenal cname with cdn_subdomain on

            string uri = api.UrlImgUp.CName("hello.com").CSubDomain(true).BuildUrl("test");
            Assert.AreEqual("http://a2.hello.com/testcloud/image/upload/test", uri);
        }

        [Test]
        public void TestHttpEscape()
        {
            string uri = api.Url.ResourceType("image").Action("youtube").BuildUrl("http://www.youtube.com/watch?v=d9NF2edxy-M");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/youtube/http://www.youtube.com/watch%3Fv%3Dd9NF2edxy-M", uri);
        }

        [Test]
        public void TestBackground()
        {
            // should support background
            Transformation transformation = new Transformation().Background("red");
            String result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/b_red/test", result);
            transformation = new Transformation().Background("#112233");
            result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/b_rgb:112233/test", result);
        }

        [Test]
        public void TestDefaultImage()
        {
            // should support default_image
            Transformation transformation = new Transformation().DefaultImage("default");
            String result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/d_default/test", result);
        }

        [Test]
        public void TestAngle()
        {
            // should support angle
            Transformation transformation = new Transformation().Angle(12);
            String result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/a_12/test", result);
            transformation = new Transformation().Angle("exif", "12");
            result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/a_exif.12/test", result);
        }

        [Test]
        public void TestOverlay()
        {
            // should support overlay
            Transformation transformation = new Transformation().Overlay("text:hello");
            String result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/l_text:hello/test", result);
            // should not pass width/height to html if overlay
            transformation = new Transformation().Overlay("text:hello").Width(100).Height(100);
            result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.IsNull(transformation.HtmlHeight);
            Assert.IsNull(transformation.HtmlWidth);
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/h_100,l_text:hello,w_100/test", result);
        }

        [Test]
        public void TestUnderlay()
        {
            Transformation transformation = new Transformation().Underlay("text:hello");
            String result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/u_text:hello/test", result);
            // should not pass width/height to html if underlay
            transformation = new Transformation().Underlay("text:hello").Width(100).Height(100);
            result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.IsNull(transformation.HtmlHeight);
            Assert.IsNull(transformation.HtmlWidth);
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/h_100,u_text:hello,w_100/test", result);
        }

        [Test]
        public void TestFetchFormat()
        {
            // should support format for fetch urls
            String result = api.UrlImgUp.Format("jpg").Action("fetch").BuildUrl("http://cloudinary.com/images/logo.png");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/fetch/f_jpg/http://cloudinary.com/images/logo.png", result);
        }

        [Test]
        public void TestEffect()
        {
            // should support effect
            Transformation transformation = new Transformation().Effect("sepia");
            String result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/e_sepia/test", result);
        }

        [Test]
        public void TestEffectWithParam()
        {
            // should support effect with param
            Transformation transformation = new Transformation().Effect("sepia", 10);
            String result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/e_sepia:10/test", result);
        }

        [Test]
        public void TestDensity()
        {
            // should support density
            Transformation transformation = new Transformation().Density(150);
            String result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/dn_150/test", result);
        }

        [Test]
        public void TestPage()
        {
            // should support page
            Transformation transformation = new Transformation().Page(5);
            String result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/pg_5/test", result);
        }

        [Test]
        public void TestBorder()
        {
            // should support border
            Transformation transformation = new Transformation().Border(5, "black");
            String result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/bo_5px_solid_black/test", result);
            transformation = new Transformation().Border(5, "#ffaabbdd");
            result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/bo_5px_solid_rgb:ffaabbdd/test", result);
            transformation = new Transformation().Border("1px_solid_blue");
            result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/bo_1px_solid_blue/test", result);
        }

        [Test]
        public void TestFlags()
        {
            // should support flags
            Transformation transformation = new Transformation().Flags("abc");
            String result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/fl_abc/test", result);
            transformation = new Transformation().Flags("abc", "def");
            result = api.UrlImgUp.Transform(transformation).BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/fl_abc.def/test", result);
        }

        [Test]
        public void TestTransformationClone()
        {
            // transformation should be cloneable
            Transformation transform1 = new Transformation().X(100).Y(100).Width(200).Crop("fill").Chain().Radius(10).Chain().Crop("crop").Width(100).Angle("12", "13", "14");
            Transformation transform2 = transform1.Clone();
            transform1 = transform1.Angle("22", "23").Chain().Crop("fill");

            Assert.AreEqual(3, transform1.NestedTransforms.Count);
            Assert.AreEqual(1, transform1.Params.Count);
            Assert.AreEqual(2, transform2.NestedTransforms.Count);
            Assert.AreEqual(3, transform2.Params.Count);
        }

        [Test]
        public void TestUrlClone()
        {
            // url should be cloneable
            Transformation t1 = new Transformation().Angle(12);
            Transformation t2 = new Transformation().Crop("fill");
            Url url1 = api.UrlImgUp.Transform(t1);
            Url url2 = url1.Clone().Action("go").Transform(t2);
            string result1 = url1.BuildUrl("test");
            string result2 = url2.BuildUrl("test");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/upload/a_12/test", result1);
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/go/c_fill/test", result2);
        }

        [Test]
        public void TestInitFromUri()
        {
            Cloudinary cloudinary = new Cloudinary("cloudinary://a:b@test123");
        }

        [Test]
        public void TestInitFromEnvironmentVariable()
        {
            Environment.SetEnvironmentVariable("CLOUDINARY_URL", "cloudinary://a:b@test123");
            Cloudinary cloudinary = new Cloudinary();
        }

        [Test]
        public void TestSecureDistributionFromUrl()
        {
            // should take secure distribution from url if secure=TRUE

            Cloudinary cloudinary = new Cloudinary("cloudinary://a:b@test123/config.secure.distribution.com");
            string url = cloudinary.Api.UrlImgUp.BuildUrl("test");

            Assert.AreEqual("https://config.secure.distribution.com/image/upload/test", url);
        }

        [Test]
        public void TestImageTag()
        {
            Transformation transformation = new Transformation().Width(100).Height(101).Crop("crop");

            StringDictionary dict = new StringDictionary();
            dict["alt"] = "my image";

            String result = api.UrlImgUp.Transform(transformation).BuildImageTag("test", dict);
            Assert.AreEqual("<img src='http://res.cloudinary.com/testcloud/image/upload/c_crop,h_101,w_100/test' alt='my image' width='100' height='101'/>", result);
        }

        [Test]
        public void TestImageUploadTag()
        {
            Dictionary<string, string> htmlOptions = new Dictionary<string, string>();
            htmlOptions.Add("htmlattr", "htmlvalue");

            string s = api.BuildUploadForm("test-field", "auto", null, htmlOptions);

            Assert.IsTrue(s.Contains("type='file'"));
            Assert.IsTrue(s.Contains("data-cloudinary-field='test-field'"));
            Assert.IsTrue(s.Contains("class='cloudinary-fileupload'"));
            Assert.IsTrue(s.Contains("htmlattr='htmlvalue'"));

            htmlOptions.Clear();
            htmlOptions.Add("class", "myclass");

            s = api.BuildUploadForm("test-field", "auto", null, htmlOptions);

            Assert.IsTrue(s.Contains("class='cloudinary-fileupload myclass'"));
        }

        [Test]
        public void TestSprite()
        {
            // should build urls to get sprite css and picture by tag (with transformations and prefix)

            string uri = api.UrlImgUp.Action("sprite").BuildUrl("teslistresourcesbytag1.png");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/sprite/teslistresourcesbytag1.png", uri);

            uri = api.UrlImgUp.Action("sprite").BuildUrl("teslistresourcesbytag1.css");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/sprite/teslistresourcesbytag1.css", uri);

            uri = api.ApiUrlImgUpV.CloudinaryAddr("http://api.cloudinary.com").Action("sprite").BuildUrl();
            Assert.AreEqual("http://api.cloudinary.com/v1_1/testcloud/image/sprite", uri);
        }

        [Test]
        public void TestSpriteTransform()
        {
            // should build urls to get sprite css and picture by tag with transformations

            Transformation t = new Transformation().Crop("fit").Height(60).Width(150);
            string uri = api.UrlImgUp.Action("sprite").Transform(t).BuildUrl("logo.png");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/sprite/c_fit,h_60,w_150/logo.png", uri);
        }

        [Test]
        public void TestSpriteCssPrefix()
        {
            // should build urls to get sprite css and picture by tag with prefix

            string uri = api.UrlImgUp.Action("sprite").Add("p_home_thing_").BuildUrl("logo.css");
            Assert.AreEqual("http://res.cloudinary.com/testcloud/image/sprite/p_home_thing_/logo.css", uri);
		}

		[Test]
		public void TestGetRequest()
		{
			var parameters = new SortedDictionary<string, object>();
			parameters.Add("start", "78668668");
			var request = api.GetRequest(HttpMethod.POST, "http://test.com", parameters, null);
			Assert.IsNotNull(request);
			Assert.IsTrue(request.ContentLength > 0);
		}

		[Test]
		public void TestGetRequestTest()
		{
			var request = api.GetTestRequest();
			Assert.IsNotNull(request);
			Assert.IsTrue(request.ContentLength > 0);
		}

		[Test]
		public void StreamTest()
		{
			var request = System.Net.HttpWebRequest.Create("http://foo.com");
			request.Method = "POST";
			request.ContentType = "application/json";
			request.ContentLength = 1;
			using (var writer = new System.IO.StreamWriter(request.GetRequestStream()))
			{
				writer.Write("{ text: \"text\"}");
				writer.Flush ();
				writer.Close ();
			}
			
			Assert.IsTrue(request.ContentLength > 0);
		}
    }
}
